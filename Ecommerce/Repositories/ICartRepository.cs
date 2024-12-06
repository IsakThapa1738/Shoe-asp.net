
namespace Ecommerce.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int ShoeId, int qty);
        Task<int> RemoveItem(int ShoeId);
        Task<Cart> GetUserCart();
        Task<int> GetCartItemCount(string userId = "");
        Task<Cart> GetCart(string userId);
        Task<bool> DoCheckout(CheckoutModel model);
    }
}