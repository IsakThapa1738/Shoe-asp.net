using System.Collections.Generic;
using YourNamespace.Models;

namespace Ecommerce.Models
{
    public class Category
    {
        // Primary Key for Category
        public int Id { get; set; }

        // Category name
        public string Name { get; set; }

       
     

        // Navigation property to Shoes
        public virtual ICollection<Shoe> Shoes { get; set; }
    }
}
