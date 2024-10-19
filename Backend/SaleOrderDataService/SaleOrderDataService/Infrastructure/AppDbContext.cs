using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;


namespace SaleOrderDataService.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        public DbSet<SaleOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderProductInfo> SalesOrderProductInfo { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            builder.Entity<SalesOrderProductInfo>()
                   .HasKey(sopi => new { sopi.ProductId, sopi.InvoiceNumber });

            builder.Entity<SalesOrderProductInfo>()
                .HasOne(sopi => sopi.Product)
                .WithMany()
                .HasForeignKey(sopi => sopi.ProductId);

            builder.Entity<SalesOrderProductInfo>()
                .HasOne(sopi => sopi.SaleOrder)
                .WithMany(so => so.Products)
                .HasForeignKey(sopi => sopi.InvoiceNumber);

            builder.Entity<SaleOrder>()
                .HasKey(so => so.InvoiceNumber);

            builder.Entity<SaleOrder>()
                .HasOne(so => so.Customer)
                .WithMany()
                .HasForeignKey(so => so.CustomerId);

            base.OnModelCreating(builder);


            //    var invoiceNumber1 = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
            //    var invoiceNumber2 = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
            //    var invoiceNumber3 = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
            //    var invoiceNumber4 = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
            //    var invoiceNumber5 = InvoiceIdGenerator.GenerateInvoiceNumber().ToString();
            //    var saleOrder1 = new SaleOrder
            //    {
            //        InvoiceNumber = invoiceNumber1,
            //        InvoiceDate = DateTime.Now,
            //        CustomerId = 1,
            //        NetTotal = 619.98m,
            //        DeliveryAddress = "123 Main Street, Anytown, USA",
            //        Tax = 50.0m,
            //        Status = OrderStatus.Processing
            //    };

            //    var saleOrder2 = new SaleOrder
            //    {
            //        InvoiceNumber = invoiceNumber2,
            //        InvoiceDate = DateTime.Now,
            //        CustomerId = 2,
            //        NetTotal = 1899.98m,
            //        DeliveryAddress = "456 Elm Street, Springfield, IL",
            //        Tax = 100.0m,
            //        Status = OrderStatus.Completed
            //    };

            //    var saleOrder3 = new SaleOrder
            //    {
            //        InvoiceNumber = invoiceNumber3,
            //        InvoiceDate = DateTime.Now,
            //        CustomerId = 3,
            //        NetTotal = 79.99m,
            //        DeliveryAddress = "789 Oak Avenue, Rivertown, CA",
            //        Tax = 10.0m,
            //        Status = OrderStatus.Shipped
            //    };

            //    var saleOrder4 = new SaleOrder
            //    {
            //        InvoiceNumber = invoiceNumber4,
            //        InvoiceDate = DateTime.Now,
            //        CustomerId = 4,
            //        NetTotal = 2499.97m,
            //        DeliveryAddress = "321 Pine Street, Lakewood, CO",
            //        Tax = 150.0m,
            //        Status = OrderStatus.Processing
            //    };

            //    var saleOrder5 = new SaleOrder
            //    {
            //        InvoiceNumber = invoiceNumber5,
            //        InvoiceDate = DateTime.Now,
            //        CustomerId = 5,
            //        NetTotal = 99.98m,
            //        DeliveryAddress = "654 Birch Lane, Frisco, TX",
            //        Tax = 5.0m,
            //        Status = OrderStatus.Pending
            //    };

            //    builder.Entity<SaleOrder>().HasData(saleOrder1);
            //    builder.Entity<SaleOrder>().HasData(saleOrder2);
            //    builder.Entity<SaleOrder>().HasData(saleOrder3);
            //    builder.Entity<SaleOrder>().HasData(saleOrder4);
            //    builder.Entity<SaleOrder>().HasData(saleOrder5);

            //    builder.Entity<SalesOrderProductInfo>().HasData(
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber1, ProductId = 1 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber1, ProductId = 2 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber2, ProductId = 2 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber2, ProductId = 1 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber2, ProductId = 3 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber3, ProductId = 4 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber3, ProductId = 5 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber4, ProductId = 5 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber4, ProductId = 3 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber5, ProductId = 3 },
            //        new SalesOrderProductInfo { InvoiceNumber = invoiceNumber5, ProductId = 1 }
            //        );

            //}

        }
    }
}
