using CustomerDataAPI.Entities;
using CustomerDataAPI.Infrastructure;
using CustomerDataAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SalesAPILibrary.Interfaces;
using System;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

//var configBuilder = new ConfigurationBuilder()
//        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load appsettings.json
//        .AddEnvironmentVariables();
//builder.Services.AddSingleton(configBuilder);


var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/users/login";  // Point to your custom login path
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
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
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
                },
                OnChallenge = context =>
                {
                    Console.WriteLine("JWT validation challenge.");
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
builder.Services.AddScoped<ICustomerDataService, CustomerDataService>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("CustomerDataAPI"))
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

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authHeader = context.Request.Headers["Authorization"];
        logger.LogInformation($"Authorization Header: {authHeader}");
    }

    await next.Invoke();
});
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowOrigin"); // Use the defined CORS policy




app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
