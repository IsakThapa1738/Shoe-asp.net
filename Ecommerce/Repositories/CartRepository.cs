using Ecommerce;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int ShoeId, int qty)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not logged-in");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await GetCart(userId) ?? new Cart { UserId = userId };
                if (cart.Id == 0)
                    _db.Carts.Add(cart);

                var cartItem = await _db.CartItems.FirstOrDefaultAsync(a => a.CartId == cart.Id && a.ShoeId == ShoeId);
                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var Shoe = await _db.Shoes.FindAsync(ShoeId)
                                  ?? throw new InvalidOperationException("Shoe not found");
                    cartItem = new CartItem
                    {
                        ShoeId = ShoeId,
                        CartId = cart.Id,
                        Quantity = qty,
                        Cart = cart,
                        Shoe = Shoe,
                        UnitPrice = Shoe.Price
                    };
                    _db.CartItems.Add(cartItem);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetCartItemCount(userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogError(ex);
                throw;
            }
        }

        public async Task<int> RemoveItem(int ShoeId)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not logged-in");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var cart = await GetCart(userId) ?? throw new InvalidOperationException("Cart not found");
                var cartItem = await _db.CartItems
                                        .FirstOrDefaultAsync(a => a.CartId == cart.Id && a.ShoeId == ShoeId);

                if (cartItem == null)
                    throw new InvalidOperationException("Item not found in cart");

                if (cartItem.Quantity == 1)
                {
                    _db.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity -= 1;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetCartItemCount(userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogError(ex);
                throw;
            }
        }

        public async Task<Cart> GetUserCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User is not logged-in");

            var cart = await _db.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Shoe)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart ?? throw new InvalidOperationException("Cart not found");
        }

        public async Task<Cart> GetCart(string userId)
        {
            return await _db.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? GetUserId() : userId;

            return await _db.CartItems
                            .Where(ci => ci.Cart.UserId == userId)
                            .CountAsync();
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var cart = await GetCart(userId) ?? throw new InvalidOperationException("Cart not found");
                var cartDetails = await _db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();

                if (!cartDetails.Any())
                    throw new InvalidOperationException("Cart is empty");

                var pendingStatus = await _db.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Pending")
                                  ?? throw new InvalidOperationException("Pending order status not found");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingStatus.Id
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                var orderDetails = new List<OrderDetail>();
                foreach (var item in cartDetails)
                {
                    var stock = await _db.Stock.FirstOrDefaultAsync(a => a.ShoeId == item.ShoeId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null for Shoe " + item.ShoeId);
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} item(s) are available in the stock for Shoe {item.ShoeId}");
                    }

                    // Decrease the number of quantity from the stock table
                    stock.Quantity -= item.Quantity;

                    // Add the order details
                    orderDetails.Add(new OrderDetail
                    {
                        ShoeId = item.ShoeId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                // Add order details after all validations
                _db.OrderDetails.AddRange(orderDetails);

                // Remove cart items after the order is placed
                _db.CartItems.RemoveRange(cartDetails);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogError(ex);
                throw;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return _userManager.GetUserId(principal);
        }

        private void LogError(Exception ex)
        {
            // Replace this with actual logging logic
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
