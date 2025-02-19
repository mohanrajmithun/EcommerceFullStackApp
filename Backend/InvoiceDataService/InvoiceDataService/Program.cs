using InvoiceDataService.Infrastructure;
using InvoiceDataService.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SalesAPILibrary.Interfaces;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using Serilog;
using Serilog.Context;


var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHealthChecks();


// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Adjust to match your frontend origin
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("InvoiceAPI"))
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

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<IInvoiceService, InvoiceService>();




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
app.MapHealthChecks("/healthz");
app.UseAuthorization();
app.UseCors("AllowOrigin"); // Use the defined CORS policy


app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("InvoiceData Service started at {Time}", DateTime.UtcNow);
});

// Log when the service is stopping
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("InvoiceData Service is stopping at {Time}", DateTime.UtcNow);
});


app.Run();
