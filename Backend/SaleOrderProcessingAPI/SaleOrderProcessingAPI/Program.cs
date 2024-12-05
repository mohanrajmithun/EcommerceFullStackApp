using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaleOrderProcessingAPI.Infrastructure;
using SaleOrderProcessingAPI.Interfaces;
using SaleOrderProcessingAPI.ServiceClients;
using SaleOrderProcessingAPI.Services;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System;
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
builder.Services.AddHttpClient<IAddressValidationService, AddressValidationService>();
builder.Services.AddSingleton<IAddressValidationService, AddressValidationService>();
builder.Services.AddTransient<ISaleOrderProcessingService, SaleOrderProcessingService>();

builder.Services.AddHttpClient<ISaleOrderDataServiceClient, SaleOrderDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5259"); // Replace with the actual base URL of the SaleOrderDataService service
});

builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SaleOrderProcessingAPI"))
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

app.Run();
