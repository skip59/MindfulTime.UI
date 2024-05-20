using System.ComponentModel.DataAnnotations;

namespace MindfulTime.UI.Models
{
    public class AuthUserModel
    {
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "От 5 до 100 символов")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Поле обязательно для ввода")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "От 3 до 100 символов")]
        public string Password { get; set; }

    }
}
