
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen( o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Desarrollo de Software",
                Version = "v1",
            });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingresar el Token",
                Type = SecuritySchemeType.ApiKey
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

        });
        builder.Services.AddHealthChecks();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = false
            };
            options.User = new UserOptions
            {
                RequireUniqueEmail = true,
                AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._"                
            };
        })
        .AddEntityFrameworkStores<AuthenticateContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Dsw2025Db;Integrated Security=True");
        });

        //Autenticación
        /*VALIDACIÓn de TOKEN*/

        //levantamos la configuración de appsettings
        var jwtConfig = builder.Configuration.GetSection("Jwt");

        //Recuperamos la Key
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("No se encontró la clave JWT en la configuración.");
       
        //pasamos la Key a bytes
        var key = Encoding.UTF8.GetBytes(keyText);
        
        builder.Services.AddAuthentication(options =>
        {
            /*esquemas por defecto a usar por el servicio de autenticación*/
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddJwtBearer(options =>
        {
            //config para token
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        builder.Services.AddSingleton<JwtTokenService>();

    
        builder.Services.AddDbContext<Dsw2025TpiContext>(option =>
        {
            option.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Dsw2025Db;Integrated Security=True");
        });

        builder.Services.AddControllers()
            .AddJsonOptions(opt =>
            {
                // <-- Esto convierte siempre los enums a sus nombres de string
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddScoped<IRepository, EfRepository>();

        builder.Services.AddScoped<ProductsManagmentService>();

        builder.Services.AddScoped<OrderManagementService>();

        builder.Services.AddControllers();

        var app = builder.Build();

        //seed clientes
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

            if (!ctx.Customers.Any())
            {
                // Ruta al JSON en el directorio de salida
                var dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
                var filePath = Path.Combine(dataFolder, "customers.json");

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"No encontré {filePath}");

                var json = File.ReadAllText(filePath);
                var customers = JsonSerializer.Deserialize<List<Customer>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (customers?.Any() == true)
                {
                    ctx.Customers.AddRange(customers);
                    ctx.SaveChanges();
                }
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
