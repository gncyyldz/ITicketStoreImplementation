using System.ComponentModel.DataAnnotations;

namespace ITicketStoreImplementation.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string? UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string? Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
