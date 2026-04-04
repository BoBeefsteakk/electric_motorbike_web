using System.Text.Json;
using VinfastWeb.Models;

namespace VinfastWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        }

        // ── Motorbikes ──────────────────────────────────────────────
        public async Task<List<Motorbike>> GetMotorbikesAsync()
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/products");
                return JsonSerializer.Deserialize<List<Motorbike>>(res, _json) ?? new();
            }
            catch { return new(); }
        }

        public async Task<Motorbike?> GetMotorbikeAsync(int id)
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/products/{id}");
                return JsonSerializer.Deserialize<Motorbike>(res, _json);
            }
            catch { return null; }
        }

        // ── Cars ────────────────────────────────────────────────────
        public async Task<List<Car>> GetCarsAsync()
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/cars");
                return JsonSerializer.Deserialize<List<Car>>(res, _json) ?? new();
            }
            catch { return new(); }
        }

        // ── Accessories ─────────────────────────────────────────────
        public async Task<List<Accessory>> GetAccessoriesAsync()
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/accessories");
                return JsonSerializer.Deserialize<List<Accessory>>(res, _json) ?? new();
            }
            catch { return new(); }
        }

        // ── Stores ──────────────────────────────────────────────────
        public async Task<List<Store>> GetStoresAsync()
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/stores");
                return JsonSerializer.Deserialize<List<Store>>(res, _json) ?? new();
            }
            catch { return new(); }
        }

        public async Task<Store?> GetStoreAsync(int id)
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/stores/{id}");
                return JsonSerializer.Deserialize<Store>(res, _json);
            }
            catch { return null; }
        }

        // ── Vouchers ────────────────────────────────────────────────
        public async Task<List<Voucher>> GetVouchersAsync()
        {
            try
            {
                var res = await _http.GetStringAsync($"{_baseUrl}/api/vouchers");
                return JsonSerializer.Deserialize<List<Voucher>>(res, _json) ?? new();
            }
            catch { return new(); }
        }

        // ── Auth ─────────────────────────────────────────────────────
        public async Task<(bool success, string? token, string? message)> LoginAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
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
            catch { return (false, null, "Lỗi kết nối server"); }
        }

        public async Task<(bool success, string? message)> RegisterAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var res = await _http.PostAsync($"{_baseUrl}/api/auth/register", content);
                var body = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);
                var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : "";
                return (res.IsSuccessStatusCode, msg);
            }
            catch { return (false, "Lỗi kết nối server"); }
        }

        // ── Proxy image URL ──────────────────────────────────────────
        public string GetImageUrl(string path) => $"{_baseUrl}/images/{path}";
    }
}