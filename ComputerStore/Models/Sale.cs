using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0; 

        public int UserId { get; set; }
        public User? User { get; set; } 

        public List<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}