using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace Ecommerce.Repositories
{
    public interface IShoeRepository
    {
        Task AddShoe(Shoe Shoe);
        Task DeleteShoe(Shoe Shoe);
        Task<Shoe?> GetShoeById(int id);
        Task<IEnumerable<Shoe>> GetShoes();
        Task UpdateShoe(Shoe Shoe);
    }

    public class ShoeRepository : IShoeRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddShoe(Shoe Shoe)
        {
            try
            {
                // Validate the Shoe manually
                if (Shoe == null)
                    throw new ArgumentNullException(nameof(Shoe));

                if (string.IsNullOrWhiteSpace(Shoe.Name))
                    throw new ArgumentException("Shoe name cannot be empty");

                if (Shoe.Price <= 0)
                    throw new ArgumentException("Price must be greater than zero");

                if (Shoe.CategoryId <= 0)
                    throw new ArgumentException("Invalid category");

                // Ensure the Shoe is not tracking an existing entity
                _context.Entry(Shoe).State = EntityState.Added;

                _context.Shoes.Add(Shoe);
                int result = await _context.SaveChangesAsync();

                Console.WriteLine($"SaveChangesAsync result: {result}");

                if (result == 0)
                    throw new Exception("No changes were saved to the database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to allow controller to handle
            }
        }

        public async Task UpdateShoe(Shoe Shoe)
        {
            _context.Shoes.Update(Shoe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteShoe(Shoe Shoe)
        {
            _context.Shoes.Remove(Shoe);
            await _context.SaveChangesAsync();
        }

        public async Task<Shoe?> GetShoeById(int id) => await _context.Shoes.FindAsync(id);

        public async Task<IEnumerable<Shoe>> GetShoes() => await _context.Shoes.Include(a => a.Category).ToListAsync();
    }
}