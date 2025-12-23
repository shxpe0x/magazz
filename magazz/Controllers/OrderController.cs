using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using magazz.Data;
using magazz.Models;

namespace magazz.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Order/MyOrders
        // Список заказов пользователя
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Находим Customer по UserId
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == userId);

            if (user?.Customer == null)
            {
                return View(new List<Order>());
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == user.Customer.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Order/Details/5
        // Детали заказа
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == userId);

            if (user?.Customer == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == user.Customer.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Order/Checkout
        // Страница оформления заказа
        public async Task<IActionResult> Checkout()
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Получаем корзину пользователя
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index", "Cart");
            }

            // Получаем данные Customer
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == userId);

            ViewBag.Cart = cart;
            ViewBag.Customer = user?.Customer;

            return View();
        }

        // POST: /Order/Create
        // Создание заказа из корзины
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string deliveryAddress, string? note)
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Получаем корзину
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index", "Cart");
            }

            // Получаем Customer
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == userId);

            if (user?.Customer == null)
            {
                TempData["Error"] = "Ошибка получения данных пользователя";
                return RedirectToAction("Index", "Cart");
            }

            // Проверяем наличие товаров
            foreach (var item in cart.CartItems)
            {
                if (!item.Product.IsAvailable || item.Product.Stock < item.Quantity)
                {
                    TempData["Error"] = $"Товар '{item.Product.Name}' недоступен в нужном количестве";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Создаем заказ
            var order = new Order
            {
                CustomerId = user.Customer.Id,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                DeliveryAddress = deliveryAddress,
                Note = note,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Создаем OrderItems
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    PriceAtOrder = cartItem.Product.Price,
                    SelectedSize = cartItem.SelectedSize,
                    SelectedColor = cartItem.SelectedColor
                };
                _context.OrderItems.Add(orderItem);

                // Уменьшаем склад
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            // Очищаем корзину
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Заказ №{order.OrderNumber} успешно оформлен!";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // Генерация уникального номера заказа
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
