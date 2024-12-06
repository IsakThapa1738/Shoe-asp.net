
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace Ecommerce.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Category>> Categories()
        {
            return await _db.Categories.ToListAsync();
        }
        public async Task<IEnumerable<Shoe>> GetShoes(string sTerm = "", int categoryId = 0)
        {
            sTerm = sTerm.ToLower();
            IEnumerable<Shoe> Shoes = await (from Shoe in _db.Shoes
                                             join category in _db.Categories
                                             on Shoe.CategoryId equals category.Id
                                             join stock in _db.Stock
                                             on Shoe.Id equals stock.ShoeId
                                             into Shoe_stocks
                                             from ShoeWithStock in Shoe_stocks.DefaultIfEmpty()
                                             where string.IsNullOrWhiteSpace(sTerm) || (Shoe != null && Shoe.Name.ToLower().StartsWith(sTerm))
                                             select new Shoe
                                             {
                                                 Id = Shoe.Id,
                                                 
                                                 Name = Shoe.Name,
                                                 CategoryId = Shoe.CategoryId,
                                                 Price = Shoe.Price,
                                                 Image = Shoe.Image,
                                                 CategoryName = category.Name,
                                                 Quantity = ShoeWithStock == null ? 0 : ShoeWithStock.Quantity
                                             }
                         ).ToListAsync();
            if (categoryId > 0)
            {

                Shoes = Shoes.Where(a => a.CategoryId == categoryId).ToList();
            }
            return Shoes;

        }
    }
}