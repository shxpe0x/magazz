using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using magazz.Data;
using magazz.Models;
using magazz.ViewModels;

namespace magazz.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Products
        // Показать каталог товаров с пагинацией
        public async Task<IActionResult> Index(int? categoryId, int? brandId, string? search, 
            string? sortBy, int page = 1, int pageSize = 12)
        {
            // Начинаем с всех товаров
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .Where(p => p.IsAvailable)
                .AsQueryable();

            // Фильтр по категории
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Фильтр по бренду
            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Поиск по названию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || 
                                        (p.Description != null && p.Description.Contains(search)));
            }

            // Сортировка
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            // Применяем пагинацию
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(query, page, pageSize);

            // Создаем ViewModel
            var viewModel = new ProductListViewModel
            {
                Products = paginatedProducts,
                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                CurrentCategoryId = categoryId,
                CurrentBrandId = brandId,
                CurrentSearch = search,
                CurrentSort = sortBy,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: /Products/Details/5
        // Показать карточку товара
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Products/Create
        // Форма создания нового товара (только для авторизованных)
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Stock,IsAvailable,CategoryId,BrandId")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Товар успешно создан!";
                return RedirectToAction(nameof(Edit), new { id = product.Id });
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            return View(product);
        }

        // GET: /Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Sizes)
                .Include(p => p.Colors)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock,IsAvailable,CategoryId,BrandId,CreatedAt")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Товар успешно обновлен!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.ToListAsync();
            return View(product);
        }

        // GET: /Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Товар успешно удален!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API для добавления размера с проверкой на дубликаты
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSize(int productId, string size, int stock)
        {
            if (string.IsNullOrWhiteSpace(size))
            {
                TempData["ErrorMessage"] = "Размер не может быть пустым";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            // Проверка на дубликаты
            var existingSize = await _context.ProductSizes
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.Size.ToUpper() == size.Trim().ToUpper());
            
            if (existingSize != null)
            {
                TempData["ErrorMessage"] = $"Размер '{size}' уже добавлен для этого товара";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            var productSize = new ProductSize
            {
                ProductId = productId,
                Size = size.Trim().ToUpper(),
                Stock = stock
            };

            // Валидация модели
            if (!TryValidateModel(productSize))
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            _context.ProductSizes.Add(productSize);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Размер '{size}' успешно добавлен";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // API для добавления цвета с проверкой на дубликаты
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddColor(int productId, string color, string? hexCode, int stock)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                TempData["ErrorMessage"] = "Цвет не может быть пустым";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            // Проверка на дубликаты
            var existingColor = await _context.ProductColors
                .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.Color.ToLower() == color.Trim().ToLower());
            
            if (existingColor != null)
            {
                TempData["ErrorMessage"] = $"Цвет '{color}' уже добавлен для этого товара";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            var productColor = new ProductColor
            {
                ProductId = productId,
                Color = color.Trim(),
                HexCode = hexCode?.Trim(),
                Stock = stock
            };

            // Валидация модели
            if (!TryValidateModel(productColor))
            {
                TempData["ErrorMessage"] = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            _context.ProductColors.Add(productColor);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Цвет '{color}' успешно добавлен";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // API для добавления изображения с загрузкой файла
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(int productId, IFormFile imageFile, string? altText, int displayOrder, bool isPrimary)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Файл изображения не выбран";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            // Проверка расширения файла
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = $"Недопустимый формат файла. Допустимы: {string.Join(", ", allowedExtensions)}";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            // Проверка размера файла (5 MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Размер файла не должен превышать 5 MB";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }

            // Создаем папку для загрузок, если не существует
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Генерируем уникальное имя файла
            var uniqueFileName = $"{productId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Если это главное изображение, сбрасываем флаг у других
            if (isPrimary)
            {
                var existingPrimary = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId && pi.IsPrimary)
                    .ToListAsync();
                
                foreach (var img in existingPrimary)
                {
                    img.IsPrimary = false;
                }
            }

            // Создаем запись в базе
            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = $"/uploads/products/{uniqueFileName}",
                AltText = altText?.Trim() ?? Path.GetFileNameWithoutExtension(imageFile.FileName),
                DisplayOrder = displayOrder,
                IsPrimary = isPrimary
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Изображение успешно загружено";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // Удаление изображения
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image != null)
            {
                // Удаляем файл с диска, если это не внешняя ссылка
                if (image.ImageUrl.StartsWith("/uploads/"))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Изображение удалено";
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
