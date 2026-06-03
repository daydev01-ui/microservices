using Blazored.LocalStorage;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddHttpClient("Gateway", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5000"));

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
