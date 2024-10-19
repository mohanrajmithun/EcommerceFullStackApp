using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using SalesInvoiceGeneratorServiceAPI.Infrastructure;
using SalesInvoiceGeneratorServiceAPI.Interfaces;
using SalesInvoiceGeneratorServiceAPI.ServiceClients;
using SalesInvoiceGeneratorServiceAPI.services;
using System;
using ISaleOrderDataServiceClient = SalesInvoiceGeneratorServiceAPI.Interfaces.ISaleOrderDataServiceClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddSingleton<ISaleOrderConsumer, SaleOrderConsumer>();
builder.Services.AddScoped<IInvoicePdfGeneratorService, InvoicePdfGeneratorService>();
builder.Services.AddScoped<ICustomerDataService, CustomerDataAPIService>();
builder.Services.AddScoped<IProductDataAPIService, ProductDataAPIService>();



builder.Services.AddHttpClient<ISaleOrderDataServiceClient, SaleOrderDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5259"); // Replace with the actual base URL of the customer repository service
});

//builder.Services.AddHttpClient<ICustomerDataServiceClient, CustomerDataServiceClient>(client =>
//{
//    client.BaseAddress = new Uri("http://customerdataapi"); // Replace with the actual base URL of the customer repository service
//});

//builder.Services.AddHttpClient<IProductDataServiceClient, ProductDataServiceClient>(client =>
//{
//    client.BaseAddress = new Uri("http://productsdataapiservice"); // Replace with the actual base URL of the product repository service
//});


//builder.Services
//       .AddIdentity<ApplicationUser, IdentityRole>(options =>
//       {
//           options.SignIn.RequireConfirmedAccount = false;
//           options.User.RequireUniqueEmail = true;
//           options.Password.RequireDigit = false;
//           options.Password.RequiredLength = 6;
//           options.Password.RequireNonAlphanumeric = false;
//           options.Password.RequireUppercase = false;
//       })
//       .AddRoles<IdentityRole>()
//       .AddEntityFrameworkStores<AppDbContext>();

//builder.Services.AddDbContext<AppDbContext>(options =>
//            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


var saleOrderConsumer = app.Services.GetRequiredService<ISaleOrderConsumer>();
saleOrderConsumer.StartListening();

app.Run();
