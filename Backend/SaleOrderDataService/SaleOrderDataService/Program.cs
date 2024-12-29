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
using Serilog;
using Serilog.Context;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()              // Adds the machine name
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341") // Replace with your Seq endpoint
    .MinimumLevel.Information()            // Adjust log level
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();
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
    client.BaseAddress = new Uri("http://customerdatapi"); // Replace with the actual base URL of the customer repository service
});

builder.Services.AddHttpClient<IProductDataServiceClient, ProductDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://productsdataapiservice"); // Replace with the actual base URL of the product repository service
});

builder.Services.AddHttpClient<ISaleOrderProcessingServiceClient, SaleOrderProcessingServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://saleorderprocessingapi"); // Replace with the actual base URL of the customer repository service
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

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");
    var traceId = context.Request.Headers["trace-id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    LogContext.PushProperty("TraceId", traceId);

    context.Response.Headers["trace-id"] = traceId;

    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authHeader = context.Request.Headers["Authorization"];
        logger.LogInformation($"Authorization Header: {authHeader}");
    }

    await next.Invoke();
});

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowOrigin"); // Use the defined CORS policy

app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("SaleOrderData Service started at {Time}", DateTime.UtcNow);
});

// Log when the service is stopping
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("SaleOrderData Service is stopping at {Time}", DateTime.UtcNow);
});


app.Run();
