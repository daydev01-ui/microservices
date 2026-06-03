using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;

namespace Frontend.Services;

public class ApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory factory, ILocalStorageService localStorage, ILogger<ApiService> logger)
    {
        _factory = factory;
        _localStorage = localStorage;
        _logger = logger;
    }

    private async Task<HttpClient> GetClientAsync()
    {
        var client = _factory.CreateClient("Gateway");
        var token = await _localStorage.GetItemAsStringAsync("jwt_token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var client = await GetClientAsync();
        var resp = await client.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<(bool Success, T? Data, string? Error)> PostAsync<T>(string url, object body)
    {
        var client = await GetClientAsync();
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync(url, content);
        var json = await resp.Content.ReadAsStringAsync();
        if (resp.IsSuccessStatusCode)
        {
            var data = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (true, data, null);
        }
        return (false, default, json);
    }

    public async Task<(bool Success, string? Error)> PutAsync(string url, object body)
    {
        var client = await GetClientAsync();
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var resp = await client.PutAsync(url, content);
        return (resp.IsSuccessStatusCode, resp.IsSuccessStatusCode ? null : await resp.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string url)
    {
        var client = await GetClientAsync();
        var resp = await client.DeleteAsync(url);
        return (resp.IsSuccessStatusCode, resp.IsSuccessStatusCode ? null : await resp.Content.ReadAsStringAsync());
    }

    public async Task<byte[]?> GetBytesAsync(string url)
    {
        var client = await GetClientAsync();
        var resp = await client.GetAsync(url);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }
}
