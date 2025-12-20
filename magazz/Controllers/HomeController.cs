using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using magazz.Data;
using magazz.Models;
using System.Diagnostics;

namespace magazz.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /
        // Главная страница с новыми товарами
        public async Task<IActionResult> Index()
        {
            // Показываем 12 последних добавленных товаров
            var newProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            // Передаем категории для навигации
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(newProducts);
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

    // ErrorViewModel для страницы ошибки
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
