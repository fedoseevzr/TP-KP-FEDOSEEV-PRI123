using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class SupplyItem
    {
        public int Id { get; set; }

        public int SupplyId { get; set; }
        public Supply Supply { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } 
    }
}