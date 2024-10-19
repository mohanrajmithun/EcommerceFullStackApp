using CustomerDataAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Shared_Entities;

namespace CustomerDataAPI.Infrastructure
{
    public class AppDbContext : IdentityDbContext<CustomerDataAPI.Entities.ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);


            //var customer1 = new Customer
            //{
            //    CustomerId = 1
            //   ,
            //    CustomerName = "Mithun"
            //   ,
            //    Address = "2331 packing iron drive, Frisco, TX-75036"
            //   ,
            //    Email = "mohanmithun005@gmail.com"
            //   ,
            //    PhoneNo = "4079677339"
            //   ,
            //    CreateDate = DateTime.Now
            //   ,
            //    IsActive = true
            //   ,
            //    UpdateDate = DateTime.Now

            //};

            //var customer2 = new Customer
            //{
            //    CustomerId = 2,
            //    CustomerName = "John Doe",
            //    Address = "456 Oak Street, Anytown, NY-12345",
            //    Email = "johndoe@example.com",
            //    PhoneNo = "1234567890",
            //    CreateDate = DateTime.Now,
            //    IsActive = true,
            //    UpdateDate = DateTime.Now
            //};

            //var customer3 = new Customer
            //{
            //    CustomerId = 3,
            //    CustomerName = "Jane Smith",
            //    Address = "789 Maple Avenue, Springfield, IL-67890",
            //    Email = "janesmith@example.com",
            //    PhoneNo = "9876543210",
            //    CreateDate = DateTime.Now,
            //    IsActive = true,
            //    UpdateDate = DateTime.Now
            //};

            //var customer4 = new Customer
            //{
            //    CustomerId = 4,
            //    CustomerName = "Alice Johnson",
            //    Address = "321 Pine Street, Rivertown, CA-54321",
            //    Email = "alicejohnson@example.com",
            //    PhoneNo = "4567890123",
            //    CreateDate = DateTime.Now,
            //    IsActive = true,
            //    UpdateDate = DateTime.Now
            //};

            //var customer5 = new Customer
            //{
            //    CustomerId = 5,
            //    CustomerName = "Bob Williams",
            //    Address = "654 Birch Lane, Lakewood, CO-98765",
            //    Email = "bobwilliams@example.com",
            //    PhoneNo = "7890123456",
            //    CreateDate = DateTime.Now,
            //    IsActive = true,
            //    UpdateDate = DateTime.Now
            //};


            //builder.Entity<Customer>().HasData(customer1
            //        );

            //builder.Entity<Customer>().HasData(customer2

            //    );

            //builder.Entity<Customer>().HasData(customer3

            //    );

            //builder.Entity<Customer>().HasData(customer4

            //    );
            //builder.Entity<Customer>().HasData(customer5

            //    );
        }
    }
}
