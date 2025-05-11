using System.ComponentModel.DataAnnotations;

namespace WebFilm.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa użytkownika nie może być dłuższa niż 50 znaków.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdź hasło.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasła nie są takie same.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
