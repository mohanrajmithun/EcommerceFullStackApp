using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaleOrderDataService.Infrastructure;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System;
using SaleOrderDataService.services;
using SaleOrderDataService.ServiceClients;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
       .AddIdentity<ApplicationUser, IdentityRole>(options =>
       {
           options.SignIn.RequireConfirmedAccount = false;
           options.User.RequireUniqueEmail = true;
           options.Password.RequireDigit = false;
           options.Password.RequiredLength = 6;
           options.Password.RequireNonAlphanumeric = false;
           options.Password.RequireUppercase = false;
       })
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder => builder.AllowAnyOrigin() // Allow all origins
                          .AllowAnyMethod() // Allow any HTTP method
                          .AllowAnyHeader()); // Allow any header
});


builder.Services.AddScoped<ISaleOrderDataService, SaleOrderDataApiService>();

builder.Services.AddHttpClient<ICustomerDataServiceClient, CustomerDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7276"); // Replace with the actual base URL of the customer repository service
});

builder.Services.AddHttpClient<IProductDataServiceClient, ProductDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7101/"); // Replace with the actual base URL of the product repository service
});

builder.Services.AddHttpClient<ISaleOrderProcessingServiceClient, SaleOrderProcessingServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7192"); // Replace with the actual base URL of the customer repository service
});



builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SaleOrderDataAPI"))
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

app.UseCors("AllowOrigin"); // Use the defined CORS policy


app.Run();
