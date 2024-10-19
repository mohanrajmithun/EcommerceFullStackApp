using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductsDataApiService.Entities;
using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Shared_Enums;
using System.Reflection.Emit;

namespace ProductsDataApiService.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {

            //builder.Entity<Product>()
            //    .HasOne(p => p.Category)
            //    .WithMany()
            //    .HasForeignKey(p => p.CategoryId);


            //builder.Entity<Product>()
            //                .Property(p => p.Price)
            //                .HasColumnType("decimal(18, 2)");
            //builder.Entity<Product>()
            //.HasOne(p => p.Inventory)
            //.WithOne(i => i.Product)
            //.HasForeignKey<ProductsInventory>(i => i.ProductId);

            base.OnModelCreating(builder);


            builder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId);

            //var category1 = new ProductCategory
            //{
            //    Id = 1,
            //    Name = "Electronics"
            //};

            //var category2 = new ProductCategory
            //{
            //    Id = 2,
            //    Name = "Clothing"
            //};

            //var category3 = new ProductCategory
            //{
            //    Id = 3,
            //    Name = "Home Appliances"
            //};

            //var category4 = new ProductCategory
            //{
            //    Id = 4,
            //    Name = "Furniture"
            //};

            //var category5 = new ProductCategory
            //{
            //    Id = 5,
            //    Name = "Sporting Goods"
            //};


            //// Product associated with Electronics category
            //var product1 = new Product
            //{
            //    ProductId = 1,
            //    ProductName = "Smartphone",
            //    ProductCode = "SPH001",
            //    productColor = ProductColour.Black,
            //    productSize = ProductSize.Small,
            //    Price = 599.99m,
            //    CategoryId = 1
            //};

            //// Product associated with Clothing category
            //var product2 = new Product
            //{
            //    ProductId = 2,
            //    ProductName = "T-Shirt",
            //    ProductCode = "TSH002",
            //    productColor = ProductColour.Red,
            //    productSize = ProductSize.Medium,
            //    Price = 19.99m,
            //    CategoryId = 2
            //};

            //// Product associated with Home Appliances category
            //var product3 = new Product
            //{
            //    ProductId = 3,
            //    ProductName = "Refrigerator",
            //    ProductCode = "REF003",
            //    productColor = ProductColour.White,
            //    productSize = ProductSize.Large,
            //    Price = 899.99m,
            //    CategoryId = 3
            //};

            //// Product associated with Furniture category
            //var product4 = new Product
            //{
            //    ProductId = 4,
            //    ProductName = "Sofa",
            //    ProductCode = "SFA004",
            //    productColor = ProductColour.Green,
            //    productSize = ProductSize.XLarge,
            //    Price = 999.99m,
            //    CategoryId = 4
            //};

            //// Product associated with Sporting Goods category
            //var product5 = new Product
            //{
            //    ProductId = 5,
            //    ProductName = "Tennis Racket",
            //    ProductCode = "TRK005",
            //    productColor = ProductColour.Blue,
            //    productSize = ProductSize.Large,
            //    Price = 79.99m,
            //    CategoryId = 5
            //};

            // Inventory for Smartphone (ProductId = 1)
            //var inventory1 = new ProductsInventory
            //{
            //    InventoryId = 1,
            //    ProductId = 1,
            //    QuantityAvailable = 50,  // 50 units of Smartphone available
            //    LastUpdated = DateTime.UtcNow
            //};

            //// Inventory for T-Shirt (ProductId = 2)
            //var inventory2 = new ProductsInventory
            //{
            //    InventoryId = 2,
            //    ProductId = 2,
            //    QuantityAvailable = 150,  // 150 units of T-Shirts available
            //    LastUpdated = DateTime.UtcNow
            //};

            //// Inventory for Refrigerator (ProductId = 3)
            //var inventory3 = new ProductsInventory
            //{
            //    InventoryId = 3,
            //    ProductId = 3,
            //    QuantityAvailable = 20,  // 20 units of Refrigerators available
            //    LastUpdated = DateTime.UtcNow
            //};

            //// Inventory for Sofa (ProductId = 4)
            //var inventory4 = new ProductsInventory
            //{
            //    InventoryId = 4,
            //    ProductId = 4,
            //    QuantityAvailable = 10,  // 10 units of Sofas available
            //    LastUpdated = DateTime.UtcNow
            //};

            //// Inventory for Tennis Racket (ProductId = 5)
            //var inventory5 = new ProductsInventory
            //{
            //    InventoryId = 5,
            //    ProductId = 5,
            //    QuantityAvailable = 100,  // 100 units of Tennis Rackets available
            //    LastUpdated = DateTime.UtcNow
            //};




            //builder.Entity<ProductCategory>().HasData(category1
            //       );

            //builder.Entity<ProductCategory>().HasData(category2

            //   );
            //builder.Entity<ProductCategory>().HasData(category3

            //   );
            //builder.Entity<ProductCategory>().HasData(category4

            //   );
            //builder.Entity<ProductCategory>().HasData(category5

            //   );


            //builder.Entity<Product>().HasData(product1

            //   );
            //builder.Entity<Product>().HasData(product2

            //   );
            //builder.Entity<Product>().HasData(product3

            //   );
            //builder.Entity<Product>().HasData(product4

            //   );
            //builder.Entity<Product>().HasData(product5

            //   );

            //builder.Entity<ProductsInventory>().HasData(inventory1);
            //builder.Entity<ProductsInventory>().HasData(inventory2);
            //builder.Entity<ProductsInventory>().HasData(inventory3);
            //builder.Entity<ProductsInventory>().HasData(inventory4);
            //builder.Entity<ProductsInventory>().HasData(inventory5);


        }

    }
}
