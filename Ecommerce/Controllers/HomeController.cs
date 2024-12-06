using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YourNamespace.Models;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;
        }

        public async Task<IActionResult> Index(string sterm = "", int categoryId = 0)
        {
            _logger.LogInformation("Index called with search term: {sterm} and categoryId: {categoryId}", sterm, categoryId);

            var Shoes = await _homeRepository.GetShoes(sterm, categoryId);
            var categories = await _homeRepository.Categories();

            if (!Shoes.Any())
            {
                _logger.LogWarning("No Shoes found.");
            }

            return View(new ShoeDisplayModel
            {
                Shoes = Shoes,
                
                Categories = categories
            });
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
