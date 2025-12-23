using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using magazz.Data;
using magazz.Models;

namespace magazz.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Получить или создать SessionId
        private string GetOrCreateSessionId()
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        // Получить или создать корзину
        private async Task<Cart> GetOrCreateCart()
        {
            Cart? cart = null;

            // Если пользователь авторизован, ищем корзину по UserId
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.Identity.Name; // Email = UserName
                
                // Проверяем, есть ли корзина пользователя
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // Если корзины нет, проверяем анонимную корзину
                if (cart == null)
                {
                    var sessionId = GetOrCreateSessionId();
                    var anonymousCart = await _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .FirstOrDefaultAsync(c => c.SessionId == sessionId);

                    if (anonymousCart != null && anonymousCart.CartItems.Any())
                    {
                        // Переносим анонимную корзину пользователю
                        anonymousCart.UserId = userId;
                        anonymousCart.SessionId = null;
                        anonymousCart.UpdatedAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                        cart = anonymousCart;
                    }
                    else
                    {
                        // Создаем новую корзину для пользователя
                        cart = new Cart { UserId = userId };
                        _context.Carts.Add(cart);
                        await _context.SaveChangesAsync();
                        
                        // Удаляем пустую анонимную корзину
                        if (anonymousCart != null)
                        {
                            _context.Carts.Remove(anonymousCart);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                // Анонимный пользователь - используем SessionId
                var sessionId = GetOrCreateSessionId();
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);

                if (cart == null)
                {
                    cart = new Cart { SessionId = sessionId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
            }

            return cart;
        }

        // GET: /Cart
        // Показать корзину
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCart();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        // Добавить товар в корзину
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? selectedSize = null, string? selectedColor = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable)
            {
                TempData["Error"] = "Товар недоступен";
                return RedirectToAction("Index", "Products");
            }

            var cart = await GetOrCreateCart();

            // Проверяем, есть ли уже такой товар с такими же параметрами
            var existingItem = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == productId && 
                ci.SelectedSize == selectedSize && 
                ci.SelectedColor == selectedColor);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedSize = selectedSize,
                    SelectedColor = selectedColor
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Товар добавлен в корзину";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        // Обновить количество
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(cartItemId);
            }

            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveFromCart
        // Удалить товар из корзины
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Clear
        // Очистить корзину
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var cart = await GetOrCreateCart();
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Корзина очищена";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Count
        // Количество товаров в корзине (для AJAX)
        public async Task<IActionResult> GetCount()
        {
            var cart = await GetOrCreateCart();
            var count = cart.CartItems.Sum(ci => ci.Quantity);
            return Json(new { count });
        }
    }
}
