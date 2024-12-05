using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);


builder.Services.AddHttpClient<ISaleOrderDataServiceClient, SaleOrderDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5259"); // Replace with the actual base URL of the customer repository service
});

builder.Services.AddHttpClient<IInvoiceServiceClient, InvoiceServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7262"); // URL of the Invoice Service API
    client.DefaultRequestHeaders.Add("Accept", "application/json");
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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SaleOrderInvoiceGeneratorServiceAPI"))
    .WithTracing(tracing =>
    {
    tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true)
        .AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("https://api.honeycomb.io/v1/traces");
            opt.Headers = "x-honeycomb-team=SU5aijCxMxzVhbFoL02BeD"; // replace with your Honeycomb API key
        });

    });

var configuration = builder.Configuration;



var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Retrieve the API key and log it
var apiKey = configuration["SendGrid:ApiKey"];
logger.LogInformation("SendGrid API Key on startup: {ApiKey}", apiKey);

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
