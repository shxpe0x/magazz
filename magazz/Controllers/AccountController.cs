using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using magazz.Models;
using magazz.ViewModels;
using magazz.Data;

namespace magazz.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Создаем Customer
                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Создаем ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    CustomerId = customer.Id,
                    RegisteredAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Автоматический вход после регистрации
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Если создание пользователя не удалось, удаляем Customer
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Аккаунт заблокирован из-за множественных неудачных попыток входа. Попробуйте позже.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                }
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Перевод ошибок Identity на русский
        private string TranslateIdentityError(string errorCode)
        {
            return errorCode switch
            {
                "DuplicateUserName" => "Пользователь с таким email уже существует",
                "DuplicateEmail" => "Пользователь с таким email уже существует",
                "InvalidEmail" => "Некорректный формат email",
                "PasswordTooShort" => "Пароль слишком короткий",
                "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать специальный символ",
                "PasswordRequiresDigit" => "Пароль должен содержать цифру",
                "PasswordRequiresLower" => "Пароль должен содержать строчную букву",
                "PasswordRequiresUpper" => "Пароль должен содержать заглавную букву",
                _ => "Произошла ошибка при регистрации"
            };
        }
    }
}
