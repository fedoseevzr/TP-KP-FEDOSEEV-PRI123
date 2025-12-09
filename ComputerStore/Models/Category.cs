using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название категории")]
        [Display(Name = "Название категории")]
        public string Name { get; set; } 


        public List<Product> Products { get; set; } = new List<Product>();
    }
}