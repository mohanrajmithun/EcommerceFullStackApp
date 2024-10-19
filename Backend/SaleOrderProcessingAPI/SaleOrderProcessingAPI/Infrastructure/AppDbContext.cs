using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Shared_Entities;

namespace SaleOrderProcessingAPI.Infrastructure
{
    public class AppDbContext : IdentityDbContext<SalesAPILibrary.Shared_Entities.ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }
        public DbSet<SaleOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderProductInfo> SalesOrdersProductInfo { get; set; }

    }
}
