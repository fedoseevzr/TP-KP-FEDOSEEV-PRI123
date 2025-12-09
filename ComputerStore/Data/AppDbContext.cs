using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<Debit> Debits { get; set; } 
    }
}