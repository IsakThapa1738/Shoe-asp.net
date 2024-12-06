﻿using YourNamespace.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Repositories
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Category>> Categories();
        Task<IEnumerable<Shoe>> GetShoes(string sTerm = "", int categoryId = 0);
    }
}
