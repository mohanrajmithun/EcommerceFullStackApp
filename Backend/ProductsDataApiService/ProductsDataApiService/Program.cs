using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductsDataApiService.Infrastructure;
using ProductsDataApiService.Services;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProductsDataApiService.interfaces;
using ProductsDataApiService.ServiceClients;

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

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecommerce API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
});



var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(options =>
        {
            //options.Authority = "validIssuer";
            //options.RequireHttpsMetadata = false;
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(symmetricSecurityKey)
                ),
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async context =>
                {
                    // Check if the error is related to token expiration
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        // Set 401 response if token has expired
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            "{\"message\": \"Token has expired.\"}");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            "{\"message\": \"Invalid token.\"}");
                    }
                },
                OnChallenge = async context =>
                {
                    if (context.AuthenticateFailure != null)
                    {
                        // Token validation failed
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"message\": \"Unauthorized request.\"}");
                    }
                    else
                    {
                        // Token missing or other challenge issues
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"message\": \"Token is required.\"}");
                    }

                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token validated successfully.");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    Console.WriteLine("JWT token received.");
                    return Task.CompletedTask;
                }
            };
        });



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

builder.Services.AddScoped<IProductDataAPIService,ProductDataAPIService>();
builder.Services.AddScoped<ICartService,CartService>();

builder.Services.AddHttpClient<ISaleOrderDataServiceClient, SaleOrderDataServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://saleorderdataservice"); // Replace with the actual base URL of the customer repository service
});

builder.Services.AddHttpClient<ISaleOrderProcessingServiceClient, SaleOrderProcessingServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://saleorderprocessingapi"); // Replace with the actual base URL of the customer repository service
});



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


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ProductsAPI"))
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

        // Configure Jaeger Exporter
        //tracing.AddJaegerExporter(jaegerOptions =>
        //{
        //    jaegerOptions.AgentHost = "localhost"; // Replace with your Jaeger instance's host if different
        //    jaegerOptions.AgentPort = 6831; // Default Jaeger agent port for UDP
        //});


    });

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Remove this section if using app.UseSerilogRequestLogging()
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var traceId = context.Request.Headers["trace-id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    LogContext.PushProperty("TraceId", traceId);

    context.Response.Headers["trace-id"] = traceId;

    await next.Invoke();
});

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

app.UseRouting();

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles(); // Enable serving static files

app.UseCors("AllowOrigin"); // Use the defined CORS policy

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("ProductsData Service started at {Time}", DateTime.UtcNow);
});

// Log when the service is stopping
app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("ProductsData Service is stopping at {Time}", DateTime.UtcNow);
});


app.Run();
