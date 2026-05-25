var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BCP Stub - Simulador API Externa",
        Version = "v1",
        Description = "Simulador de la API del BCP para desarrollo y pruebas. " +
                      "Comportamientos configurables: 'confirmar', 'rechazar', 'timeout', 'error', 'aleatorio'"
    });
});
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BCP Stub v1"));
app.MapControllers();
app.Run();
