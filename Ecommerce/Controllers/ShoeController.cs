using Ecommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YourNamespace.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Data.Migrations;

namespace Ecommerce.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class ShoeController : Controller
    {
        private readonly IShoeRepository _ShoeRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ShoeController(IShoeRepository ShoeRepo, ICategoryRepository categoryRepo)
        {
            _ShoeRepo = ShoeRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index()
        {
            var Shoes = await _ShoeRepo.GetShoes();
            return View(Shoes);
        }

        // AddShoe GET method
        public async Task<IActionResult> AddShoe()
        {
            var categorySelectList = (await _categoryRepo.GetCategories()).Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
            }).ToList();

            ShoeDTO ShoeToAdd = new()
            {
                CategoryList = categorySelectList
            };
            return View(ShoeToAdd);
        }

        // AddShoe POST method
        [HttpPost]
        public async Task<IActionResult> AddShoe(ShoeDTO ShoeToAdd)
        {
            // Populate the category list for the dropdown
            var categorySelectList = (await _categoryRepo.GetCategories()).Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
            }).ToList();
            ShoeToAdd.CategoryList = categorySelectList;

            if (!ModelState.IsValid)
            {
                TempData["errorMessage"] = "Validation failed. Please correct the highlighted errors.";
                return View(ShoeToAdd);
            }

            try
            {
                // Handle image upload if provided
                if (ShoeToAdd.ImageFile != null)
                {
                    if (ShoeToAdd.ImageFile.Length > 1 * 1024 * 1024) // 1 MB limit for image size
                    {
                        throw new InvalidOperationException("Image file size cannot exceed 1 MB.");
                    }

                    // Save the image file to wwwroot/images/
                    var wwwPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");

                    if (!Directory.Exists(wwwPath))
                    {
                        Directory.CreateDirectory(wwwPath); // Create the directory if it doesn't exist
                    }

                    string fileExtension = Path.GetExtension(ShoeToAdd.ImageFile.FileName); // Get the file extension
                    string fileName = $"{Guid.NewGuid()}{fileExtension}"; // Generate a unique name for the file
                    string filePath = Path.Combine(wwwPath, fileName); // Full path where the file will be saved

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ShoeToAdd.ImageFile.CopyToAsync(fileStream); // Copy the file to the path
                    }

                    ShoeToAdd.Image = fileName; // Save the filename to the DTO
                }

                // Map DTO to Shoe entity
                Shoe Shoe = new()
                {
                    Name = ShoeToAdd.Name,
                    Image = ShoeToAdd.Image, // Use the file name saved above
                    Description = ShoeToAdd.Description,
                    CategoryId = ShoeToAdd.CategoryId,
                    Price = ShoeToAdd.Price
                };

                // Save the Shoe to the database
             
                    await _ShoeRepo.AddShoe(Shoe);
                    TempData["successMessage"] = "Shoe added successfully.";
                    return RedirectToAction(nameof(AddShoe));
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = $"An error occurred: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        TempData["errorMessage"] += $" Inner Exception: {ex.InnerException.Message}";
                    }
                    return View(ShoeToAdd);
                }

            }

        // UpdateShoe GET method
        public async Task<IActionResult> UpdateShoe(int id)
        {
            var Shoe = await _ShoeRepo.GetShoeById(id);
            if (Shoe == null)
            {
                TempData["errorMessage"] = $"Shoe with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            var categorySelectList = (await _categoryRepo.GetCategories()).Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
                Selected = category.Id == Shoe.CategoryId
            });

            ShoeDTO ShoeToUpdate = new()
            {
                Name = Shoe.Name,
                CategoryId = Shoe.CategoryId,
                Price = Shoe.Price,
                Image = Shoe.Image,
                Description = Shoe.Description,
                CategoryList = categorySelectList
            };

            return View(ShoeToUpdate);
        }

        // UpdateShoe POST method
        [HttpPost]
        public async Task<IActionResult> UpdateShoe(ShoeDTO ShoeToUpdate)
        {
            var categorySelectList = (await _categoryRepo.GetCategories()).Select(category => new SelectListItem
            {
                Text = category.Name,
                Value = category.Id.ToString(),
                Selected = category.Id == ShoeToUpdate.CategoryId
            });
            ShoeToUpdate.CategoryList = categorySelectList;

            if (!ModelState.IsValid)
                return View(ShoeToUpdate);

            try
            {
                string oldImage = ShoeToUpdate.Image;

                if (ShoeToUpdate.ImageFile != null)
                {
                    if (ShoeToUpdate.ImageFile.Length > 1 * 1024 * 1024) // 1 MB limit
                    {
                        throw new InvalidOperationException("Image file cannot exceed 1 MB.");
                    }

                    // Save the image file to wwwroot/images/
                    var wwwPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");

                    if (!Directory.Exists(wwwPath))
                    {
                        Directory.CreateDirectory(wwwPath); // Create the directory if it doesn't exist
                    }

                    string fileExtension = Path.GetExtension(ShoeToUpdate.ImageFile.FileName); // Get the file extension
                    string fileName = $"{Guid.NewGuid()}{fileExtension}"; // Generate a unique name for the file
                    string filePath = Path.Combine(wwwPath, fileName); // Full path where the file will be saved

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ShoeToUpdate.ImageFile.CopyToAsync(fileStream); // Copy the file to the path
                    }

                    ShoeToUpdate.Image = fileName; // Save the filename to the DTO
                }

                Shoe Shoe = new()
                {
                    Id = ShoeToUpdate.Id,
                    Name = ShoeToUpdate.Name,
                    CategoryId = ShoeToUpdate.CategoryId,
                    Description = ShoeToUpdate.Description,
                    Price = ShoeToUpdate.Price,
                    Image = ShoeToUpdate.Image
                };

                await _ShoeRepo.UpdateShoe(Shoe);

                // Delete the old image if a new one is provided
                if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != ShoeToUpdate.Image)
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", oldImage);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                TempData["successMessage"] = "Shoe was updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(ShoeToUpdate);
            }
        }

        // DeleteShoe method
        public async Task<IActionResult> DeleteShoe(int id)
        {
            try
            {
                var Shoe = await _ShoeRepo.GetShoeById(id);
                if (Shoe == null)
                {
                    TempData["errorMessage"] = $"Shoe with ID {id} not found.";
                }
                else
                {
                    await _ShoeRepo.DeleteShoe(Shoe);

                    // Delete the Shoe image
                    if (!string.IsNullOrWhiteSpace(Shoe.Image))
                    {
                        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", Shoe.Image);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    TempData["successMessage"] = "Shoe was deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
