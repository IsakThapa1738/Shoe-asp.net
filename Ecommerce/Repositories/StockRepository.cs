using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> GetStockByShoeId(int ShoeId) => await _context.Stock.FirstOrDefaultAsync(s => s.ShoeId == ShoeId);

        public async Task ManageStock(StockDTO stockToManage)
        {
            
            var existingStock = await GetStockByShoeId(stockToManage.ShoeId);
            if (existingStock is null)
            {
                var stock = new Stock { ShoeId = stockToManage.ShoeId, Quantity = stockToManage.Quantity };
                _context.Stock.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "")
        {
            var stocks = await (from Shoe in _context.Shoes
                                join stock in _context.Stock
                                on Shoe.Id equals stock.ShoeId
                                into Shoe_stock
                                from ShoeStock in Shoe_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(sTerm) || Shoe.Name.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    ShoeId = Shoe.Id,
                                    ShoeName = Shoe.Name,
                                    Quantity = ShoeStock == null ? 0 : ShoeStock.Quantity
                                }
                                ).ToListAsync();
            return stocks;
        }

    }

    public interface IStockRepository
    {
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task<Stock?> GetStockByShoeId(int ShoeId);
        Task ManageStock(StockDTO stockToManage);
    }
}