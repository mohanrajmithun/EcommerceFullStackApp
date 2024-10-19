using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Shared_Entities;

namespace SalesInvoiceGeneratorServiceAPI.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        public DbSet<SaleOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderProductInfo> SalesOrderProductInfo { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductCategory> ProductCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SalesOrderProductInfo>()
                .HasKey(s => new { s.ProductId, s.InvoiceNumber }); // Composite key

            base.OnModelCreating(modelBuilder);
        }

    }
}
