using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Lütfen bir e-posta adresi giriniz.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }  
        
        [Required(ErrorMessage = "Lütfen bir parola giriniz.")]
        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage = "Parola en fazla 50 karakter olabilir.")]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}
