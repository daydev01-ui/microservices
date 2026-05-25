using System.Text;
using BCP.Payments.API.API.Middleware;
using BCP.Payments.API.Application.UseCases;
using BCP.Payments.API.Domain.Interfaces;
using BCP.Payments.API.Infrastructure.ExternalServices;
using BCP.Payments.API.Infrastructure.Messaging;
using BCP.Payments.API.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// === Base de datos ===
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "transactions")));

// === JWT ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });
builder.Services.AddAuthorization();

// === Cliente HTTP al BCP Stub ===
var bcpStubUrl = builder.Configuration["BCPStubUrl"] ?? "http://bcp-stub:5010";
builder.Services.AddHttpClient<IBCPExternalService, BCPHttpClient>(client =>
{
    client.BaseAddress = new Uri(bcpStubUrl);
    client.Timeout = TimeSpan.FromSeconds(4); // Margen sobre los 3s del use case
});

// === RabbitMQ con MassTransit ===
var rabbitMqUrl = builder.Configuration["RabbitMQ:Url"] ?? "rabbitmq://rabbitmq";
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(rabbitMqUrl);
        cfg.ConfigureEndpoints(ctx);
    });
});

// === Repositorios y Use Cases ===
builder.Services.AddScoped<ITransaccionRepository, TransaccionRepository>();
builder.Services.AddScoped<IEventPublisher, RabbitMQEventPublisher>();
builder.Services.AddScoped<VerificarPagoQRUseCase>();

// === API ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BCP Payments API",
        Version = "v1",
        Description = "Servicio de verificación de pagos QR"
    });
});
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplicar migraciones
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    try { db.Database.Migrate(); }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "Error al aplicar migraciones de Payments");
    }
}

app.Run();
