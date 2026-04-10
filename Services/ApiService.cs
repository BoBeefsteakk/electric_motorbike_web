using System.Text.Json;
using VinfastWeb.Models;

namespace VinfastWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly ILogger<ApiService> _logger;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IConfiguration config, ILogger<ApiService> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        private async Task<List<T>> GetListAsync<T>(string url)
        {
            try
            {
                var res = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<List<T>>(res, _json) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError("API Error [{url}]: {msg}", url, ex.Message);
                return new();
            }
        }

        public Task<List<Motorbike>> GetMotorbikesAsync() => GetListAsync<Motorbike>($"{_baseUrl}/api/products");
        public Task<List<Car>> GetCarsAsync() => GetListAsync<Car>($"{_baseUrl}/api/cars");
        public Task<List<Accessory>> GetAccessoriesAsync() => GetListAsync<Accessory>($"{_baseUrl}/api/accessories");
        public Task<List<Store>> GetStoresAsync() => GetListAsync<Store>($"{_baseUrl}/api/stores");
        public Task<List<Voucher>> GetVouchersAsync() => GetListAsync<Voucher>($"{_baseUrl}/api/vouchers");

        public async Task<Motorbike?> GetMotorbikeAsync(int id)
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/products/{id}");
                return JsonSerializer.Deserialize<Motorbike>(res, _json);
            }
            catch (Exception ex) { _logger.LogError("GetMotorbike {id}: {msg}", id, ex.Message); return null; }
        }

        public async Task<Store?> GetStoreAsync(int id)
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/stores/{id}");
                return JsonSerializer.Deserialize<Store>(res, _json);
            }
            catch (Exception ex) { _logger.LogError("GetStore {id}: {msg}", id, ex.Message); return null; }
        }

        public async Task<(bool success, string? token, string? message)> LoginAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var res = await _http.PostAsync($"{_baseUrl}/api/auth/login", content);
                var body = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);
                if (res.IsSuccessStatusCode)
                {
                    var token = doc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
                    return (true, token, null);
                }
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "Đăng nhập thất bại";
                return (false, null, msg);
            }
            catch (Exception ex) { return (false, null, ex.Message); }
        }

        public async Task<(bool success, string? message)> RegisterAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var res = await _http.PostAsync($"{_baseUrl}/api/auth/register", content);
                var body = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "";
                return (res.IsSuccessStatusCode, msg);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}