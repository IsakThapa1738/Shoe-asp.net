﻿using YourNamespace.Models;

namespace Ecommerce.Models
{
    public class OrderDetail
    {
        public int Id { get; set; } // Primary Key
        public int OrderId { get; set; } // Foreign Key to Order
        public int ShoeId { get; set; } // Foreign Key to Shoe
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } // Optional: Discount applied to the Shoe
        public decimal TotalPrice { get; set; } // Calculated as Quantity * UnitPrice - Discount

        
        // Navigation properties
        public Order Order { get; set; }
        public Shoe Shoe { get; set; }
    }

}
