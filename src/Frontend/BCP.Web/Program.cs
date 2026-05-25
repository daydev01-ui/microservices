using BCP.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredLocalStorage();

// HttpClient configurado para el API Gateway
var gatewayUrl = builder.Configuration["GatewayUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient("BcpApi", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient("BcpApiFileDownload", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Servicios de la aplicación
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<NotificacionesService>();
builder.Services.AddScoped<AuthenticationStateProvider, BcpAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
