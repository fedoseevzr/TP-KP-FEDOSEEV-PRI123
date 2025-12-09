using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Поставщик")]
        public string Name { get; set; } 

        [Required]
        public string INN { get; set; } 

        public string Phone { get; set; } 

        public List<Product> Products { get; set; } = new List<Product>();
        public List<Supply> Supplies { get; set; } = new List<Supply>();
    }
}