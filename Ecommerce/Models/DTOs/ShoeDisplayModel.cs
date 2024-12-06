using YourNamespace.Models;

namespace Ecommerce.Models.DTOs
{
    public class ShoeDisplayModel
    {
        public IEnumerable<Shoe> Shoes { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        public string STerm { get; set; } = "";
        public int CategoryId { get; set; } = 0;

    }

    
}
