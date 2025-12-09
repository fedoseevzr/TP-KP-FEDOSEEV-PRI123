using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "¬ведите логин")]
        [Display(Name = "Ћогин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "¬ведите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "ѕароль")]
        public string Password { get; set; }
    }
}