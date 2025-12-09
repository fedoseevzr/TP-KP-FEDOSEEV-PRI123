using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите артикул")]
        [Display(Name = "Артикул (Код)")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите наименование")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Display(Name = "Цена")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Цена должна быть положительной")]
        public decimal Price { get; set; }

        [Display(Name = "Остаток")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Выберите поставщика")]
        [Display(Name = "Поставщик")]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public List<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public List<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
    }
}