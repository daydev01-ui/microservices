using System.Text;
using BCP.Auth.API.API.Middleware;
using BCP.Auth.API.Application.UseCases;
using BCP.Auth.API.Domain.Interfaces;
using BCP.Auth.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// === Base de datos ===
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

// === JWT ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// === Servicios de la aplicación ===
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<LoginUseCase>();

// === API ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BCP Auth API",
        Version = "v1",
        Description = "Servicio de autenticación - Sistema de Cobros QR BCP"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// === Middleware ===
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BCP Auth API v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// === Migraciones automáticas + seed de datos ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.Migrate();
        SeedDemoUsers(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones de Auth");
    }
}

static void SeedDemoUsers(BCP.Auth.API.Infrastructure.Persistence.AuthDbContext db,
    ILogger logger)
{
    var usuarios = new[]
    {
        (Id: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Password: "Admin123!"),
        (Id: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Password: "Gerente123!"),
        (Id: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), Password: "Supervisor123!"),
        (Id: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), Password: "Operador123!"),
    };

    bool changed = false;
    foreach (var (id, password) in usuarios)
    {
        var user = db.Usuarios.Find(id);
        if (user == null) continue;
        // Actualizar si el hash es un placeholder (no empieza con un hash BCrypt válido)
        if (!user.PasswordHash.StartsWith("$2a$") && !user.PasswordHash.StartsWith("$2b$"))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            changed = true;
            logger.LogInformation("Hash de contraseña actualizado para usuario {Id}", id);
        }
    }
    if (changed) db.SaveChanges();
}

app.Run();
