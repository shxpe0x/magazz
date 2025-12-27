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

        public ProductsController(AppDbContext context)
        {
            _context = context;
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

        // API для добавления размера
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSize(int productId, string size, int stock)
        {
            if (string.IsNullOrWhiteSpace(size))
            {
                return BadRequest("Размер не может быть пустым");
            }

            var productSize = new ProductSize
            {
                ProductId = productId,
                Size = size.Trim(),
                Stock = stock
            };

            _context.ProductSizes.Add(productSize);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // API для добавления цвета
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddColor(int productId, string color, string? hexCode, int stock)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return BadRequest("Цвет не может быть пустым");
            }

            var productColor = new ProductColor
            {
                ProductId = productId,
                Color = color.Trim(),
                HexCode = hexCode?.Trim(),
                Stock = stock
            };

            _context.ProductColors.Add(productColor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // API для добавления изображения
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(int productId, string imageUrl, string? altText, int displayOrder, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest("URL изображения не может быть пустым");
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

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl.Trim(),
                AltText = altText?.Trim(),
                DisplayOrder = displayOrder,
                IsPrimary = isPrimary
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
