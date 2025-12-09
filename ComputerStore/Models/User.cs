using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "¬ведите логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "¬ведите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }

        public List<Sale> Sales { get; set; } = new List<Sale>();
        public List<Supply> Supplies { get; set; } = new List<Supply>();
    }
}