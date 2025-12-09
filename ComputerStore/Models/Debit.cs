using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComputerStore.Models; 

namespace ComputerStore.Models
{

    public class Debit
    {
        public int Id { get; set; }

        [Display(Name = "Дата списания")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Количество")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        public int Quantity { get; set; }

        [Display(Name = "Причина списания")]
        [Required(ErrorMessage = "Укажите причину списания")]
        [StringLength(250, ErrorMessage = "Длина причины не должна превышать 250 символов.")] 
        public string Reason { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}