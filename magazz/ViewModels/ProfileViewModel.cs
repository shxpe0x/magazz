using System.ComponentModel.DataAnnotations;

namespace magazz.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Имя")]
        [StringLength(100, ErrorMessage = "Имя не может быть длиннее 100 символов")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [StringLength(100, ErrorMessage = "Фамилия не может быть длиннее 100 символов")]
        public string? LastName { get; set; }

        [Display(Name = "Телефон")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [StringLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        public string? Phone { get; set; }

        [Display(Name = "Адрес")]
        [StringLength(200, ErrorMessage = "Адрес не может быть длиннее 200 символов")]
        public string? Address { get; set; }

        // Для смены пароля
        [Display(Name = "Текущий пароль")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Новый пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов", MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Display(Name = "Подтвердите новый пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string? ConfirmPassword { get; set; }
    }
}
