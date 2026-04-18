using System.Net.Http.Headers;
using System.Text;
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

        // =========================
        // COMMON HELPERS
        // =========================

        private async Task<List<T>> GetListAsync<T>(string url)
        {
            try
            {
                var res = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<List<T>>(res, _json) ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error GET LIST [{Url}]", url);
                return new List<T>();
            }
        }

        private async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var res = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<T>(res, _json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error GET [{Url}]", url);
                return default;
            }
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url, string? token = null, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);

            if (!string.IsNullOrWhiteSpace(token))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body != null)
            {
                req.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );
            }

            return req;
        }

        private async Task<bool> SendAsync(HttpRequestMessage req)
        {
            try
            {
                var res = await _http.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error SEND [{Method}] [{Url}]", req.Method, req.RequestUri);
                return false;
            }
        }

        private async Task<(bool success, string? message)> SendWithMessageAsync(HttpRequestMessage req)
        {
            try
            {
                var res = await _http.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();

                string? msg = null;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("message", out var m))
                        {
                            msg = m.GetString();
                        }
                    }
                    catch
                    {
                        msg = body;
                    }
                }

                return (res.IsSuccessStatusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Error SEND MSG [{Method}] [{Url}]", req.Method, req.RequestUri);
                return (false, ex.Message);
            }
        }

        // =========================
        // PUBLIC API - USER SIDE
        // =========================

        public Task<List<Motorbike>> GetMotorbikesAsync()
            => GetListAsync<Motorbike>($"{_baseUrl}/api/products");

        public Task<List<Car>> GetCarsAsync()
            => GetListAsync<Car>($"{_baseUrl}/api/cars");

        public Task<List<Accessory>> GetAccessoriesAsync()
            => GetListAsync<Accessory>($"{_baseUrl}/api/accessories");

        public Task<List<Store>> GetStoresAsync()
            => GetListAsync<Store>($"{_baseUrl}/api/stores");

        public Task<List<Voucher>> GetVouchersAsync()
            => GetListAsync<Voucher>($"{_baseUrl}/api/vouchers");

        public async Task<Motorbike?> GetMotorbikeAsync(int id)
        {
            return await GetAsync<Motorbike>($"{_baseUrl}/api/products/{id}");
        }

        public async Task<Store?> GetStoreAsync(int id)
        {
            return await GetAsync<Store>($"{_baseUrl}/api/stores/{id}");
        }

        // =========================
        // AUTH
        // =========================

        public async Task<(bool success, string? token, string? message)> LoginAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var res = await _http.PostAsync($"{_baseUrl}/api/auth/login", content);
                var body = await res.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return (false, null, "Phản hồi từ server rỗng");
                }

                var doc = JsonDocument.Parse(body);

                if (res.IsSuccessStatusCode)
                {
                    var token = doc.RootElement.TryGetProperty("token", out var t)
                        ? t.GetString()
                        : null;

                    return (true, token, null);
                }

                var msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : "Đăng nhập thất bại";

                return (false, null, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync error");
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool success, string? message)> RegisterAsync(string account, string password)
        {
            try
            {
                var payload = new { account, password };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var res = await _http.PostAsync($"{_baseUrl}/api/auth/register", content);
                var body = await res.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return (res.IsSuccessStatusCode, res.IsSuccessStatusCode ? "Đăng ký thành công" : "Đăng ký thất bại");
                }

                var doc = JsonDocument.Parse(body);
                var msg = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : (res.IsSuccessStatusCode ? "Đăng ký thành công" : "Đăng ký thất bại");

                return (res.IsSuccessStatusCode, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync error");
                return (false, ex.Message);
            }
        }

        // =========================
        // ADMIN PRODUCT API THẬT
        // =========================

        public async Task<List<AdminProduct>> GetAdminProductsAsync(string? keyword = null, string? type = null)
        {
            try
            {
                var url = $"{_baseUrl}/api/admin/products";
                var query = new List<string>();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query.Add($"keyword={Uri.EscapeDataString(keyword)}");
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query.Add($"type={Uri.EscapeDataString(type)}");
                }

                if (query.Any())
                {
                    url += "?" + string.Join("&", query);
                }

                var res = await _http.GetStringAsync(url);
                return JsonSerializer.Deserialize<List<AdminProduct>>(res, _json) ?? new List<AdminProduct>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminProductsAsync error");
                return new List<AdminProduct>();
            }
        }

        public async Task<AdminProduct?> GetAdminProductAsync(int id)
        {
            try
            {
                var (type, rawId) = ResolveAdminTypeAndId(id);
                var res = await _http.GetStringAsync($"{_baseUrl}/api/admin/products/{type}/{rawId}");

                using var doc = JsonDocument.Parse(res);
                return MapAdminProductDetail(doc.RootElement, type, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminProductAsync {Id} error", id);
                return null;
            }
        }

        public async Task<bool> CreateAdminProductAsync(AdminProduct model, string? token)
        {
            var payload = new
            {
                productType = model.ProductType switch
                {
                    "Xe máy" => "motorbike",
                    "Ô tô" => "car",
                    "Phụ kiện" => "accessory",
                    "Showroom" => "store",
                    _ => "motorbike"
                },

                name = model.ProductName,

                price = model.Price,

                image = string.IsNullOrWhiteSpace(model.PathImage)
                        ? null
                        : model.PathImage,

                category = model.ProductType == "Xe máy"
                            ? model.CategoryGroup?.ToLower()
                                .Replace(" ", "_")
                                .Replace("ổ", "o")
                                .Replace("ấ", "a")
                            : null,

                is_featured = 0
            };

            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products",
                token,
                payload
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<bool> UpdateAdminProductAsync(AdminProduct model, string? token)
        {
            var (type, rawId) = ResolveAdminTypeAndId(model.ProductID);

            var payload = new
            {
                name = model.ProductName,
                price = model.Price,
                image = string.IsNullOrWhiteSpace(model.PathImage) ? null : model.PathImage,
                category = type == "motorbike" ? model.CategoryGroup : null,
                is_featured = 0
            };

            var req = CreateRequest(
                HttpMethod.Put,
                $"{_baseUrl}/api/admin/products/{type}/{rawId}",
                token,
                payload
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<bool> DeleteAdminProductAsync(int id, string? token)
        {
            var (type, rawId) = ResolveAdminTypeAndId(id);

            var req = CreateRequest(
                HttpMethod.Delete,
                $"{_baseUrl}/api/admin/products/{type}/{rawId}",
                token
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<bool> DeleteManyAdminProductsAsync(List<int> ids, string? token)
        {
            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products/bulk-delete",
                token,
                ids
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<bool> DuplicateAdminProductAsync(int id, string? token)
        {
            var (type, rawId) = ResolveAdminTypeAndId(id);

            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products/duplicate/{type}/{rawId}",
                token
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<bool> DuplicateManyAdminProductsAsync(List<int> ids, string? token)
        {
            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products/bulk-duplicate",
                token,
                ids
            );

            using (req)
            {
                return await SendAsync(req);
            }
        }

        public async Task<(bool success, string? message)> ExportAdminProductsJsonAsync(string? token, string filePath)
        {
            try
            {
                using var req = CreateRequest(
                    HttpMethod.Get,
                    $"{_baseUrl}/api/admin/products/export/json",
                    token
                );

                using var res = await _http.SendAsync(req);

                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    return (false, string.IsNullOrWhiteSpace(err) ? "Export thất bại" : err);
                }

                var bytes = await res.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, bytes);

                return (true, "Export thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportAdminProductsJsonAsync error");
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string? message)> ImportAdminProductsJsonAsync(string? token, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, "Không tìm thấy file import");
                }

                using var form = new MultipartFormDataContent();
                await using var stream = File.OpenRead(filePath);
                using var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                form.Add(fileContent, "file", Path.GetFileName(filePath));

                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/admin/products/import/json");
                req.Content = form;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var res = await _http.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    return (true, string.IsNullOrWhiteSpace(body) ? "Import thành công" : body);
                }

                return (false, string.IsNullOrWhiteSpace(body) ? "Import thất bại" : body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportAdminProductsJsonAsync error");
                return (false, ex.Message);
            }
        }

        // =========================
        // ADMIN PRODUCT - MERGE TỪ DATA HIỆN CÓ
        // =========================

        private string NormalizeCategory(string? rawCategory, string sourceType)
        {
            var c = (rawCategory ?? "").Trim().ToLower();

            if (sourceType == "Ô tô")
                return "Ô tô";

            if (sourceType == "Phụ kiện")
                return "Phụ kiện";

            if (sourceType == "Showroom")
                return "Showroom";

            if (c.Contains("pho") || c.Contains("phổ"))
                return "Phổ thông";

            if (c.Contains("trung"))
                return "Trung cấp";

            if (c.Contains("cao"))
                return "Cao cấp";

            return sourceType;
        }

        public async Task<List<AdminProduct>> GetAdminProductsMergedAsync(string? keyword = null, string? productType = null)
        {
            try
            {
                var result = new List<AdminProduct>();

                var bikes = await GetMotorbikesAsync();
                if (bikes != null && bikes.Any())
                {
                    result.AddRange(bikes.Select(x => new AdminProduct
                    {
                        ProductID = x.Id,
                        ProductName = x.Name ?? "",
                        PathImage = x.Image ?? "",
                        ProductType = "Xe máy",
                        CategoryGroup = NormalizeCategory(x.Category, "Xe máy"),
                        Quantity = 1,
                        Unit = "chiếc",
                        Price = x.Price,
                        Description = x.Category ?? "",
                        CreateDate = DateTime.Today,
                        UpdateDate = DateTime.Today
                    }));
                }

                var cars = await GetCarsAsync();
                if (cars != null && cars.Any())
                {
                    result.AddRange(cars.Select(x => new AdminProduct
                    {
                        ProductID = 100000 + x.Id,
                        ProductName = x.Name ?? "",
                        PathImage = x.Image ?? "",
                        ProductType = "Ô tô",
                        CategoryGroup = "Ô tô",
                        Quantity = 1,
                        Unit = "chiếc",
                        Price = x.Price,
                        Description = x.Category ?? "",
                        CreateDate = DateTime.Today,
                        UpdateDate = DateTime.Today
                    }));
                }

                var accessories = await GetAccessoriesAsync();
                if (accessories != null && accessories.Any())
                {
                    result.AddRange(accessories.Select(x => new AdminProduct
                    {
                        ProductID = 200000 + x.Id,
                        ProductName = x.Name ?? "",
                        PathImage = x.Image ?? "",
                        ProductType = "Phụ kiện",
                        CategoryGroup = "Phụ kiện",
                        Quantity = 1,
                        Unit = "món",
                        Price = x.Price,
                        Description = x.Name ?? "",
                        CreateDate = DateTime.Today,
                        UpdateDate = DateTime.Today
                    }));
                }

                var stores = await GetStoresAsync();
                if (stores != null && stores.Any())
                {
                    result.AddRange(stores.Select(x => new AdminProduct
                    {
                        ProductID = 300000 + x.Id,
                        ProductName = x.Name ?? "",
                        PathImage = x.Image ?? "",
                        ProductType = "Showroom",
                        CategoryGroup = "Showroom",
                        Quantity = 1,
                        Unit = "chi nhánh",
                        Price = 0,
                        Description = x.Address ?? "Địa chỉ đang cập nhật",
                        CreateDate = DateTime.Today,
                        UpdateDate = DateTime.Today
                    }));
                }

                if (!string.IsNullOrWhiteSpace(productType))
                {
                    result = result
                        .Where(x => string.Equals(x.CategoryGroup, productType, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim().ToLower();

                    result = result.Where(x =>
                            (!string.IsNullOrWhiteSpace(x.ProductName) && x.ProductName.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrWhiteSpace(x.Description) && x.Description.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrWhiteSpace(x.CategoryGroup) && x.CategoryGroup.ToLower().Contains(keyword)) ||
                            x.ProductID.ToString().Contains(keyword)
                        )
                        .ToList();
                }

                return result
                    .OrderByDescending(x => x.ProductID)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminProductsMergedAsync error");
                return new List<AdminProduct>();
            }
        }
        private string NormalizeImagePath(string? rawPath, string folder)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                return $"/images/{folder}/no-image.png";

            var path = rawPath.Replace("\\", "/").Trim();

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return path;

            // Nếu DB đã đúng kiểu /images/...
            if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace("/images/cars/", "/images/car/", StringComparison.OrdinalIgnoreCase);
                path = path.Replace("/images/motorbikes/", "/images/motorbike/", StringComparison.OrdinalIgnoreCase);
                path = path.Replace("/images/accessories/", "/images/accessory/", StringComparison.OrdinalIgnoreCase);
                return path;
            }

            // Nếu DB chỉ lưu tên file
            return $"/images/{folder}/{path.TrimStart('/')}";
        }

        private (string type, int rawId) ResolveAdminTypeAndId(int productId)
        {
            if (productId >= 300000)
                return ("store", productId - 300000);

            if (productId >= 200000)
                return ("accessory", productId - 200000);

            if (productId >= 100000)
                return ("car", productId - 100000);

            return ("motorbike", productId);
        }

        private AdminProduct MapAdminProductDetail(JsonElement root, string type, int displayId)
        {
            string? GetString(string name)
                => root.TryGetProperty(name, out var p) && p.ValueKind != JsonValueKind.Null
                    ? p.GetString()
                    : null;

            decimal GetDecimal(string name)
            {
                if (!root.TryGetProperty(name, out var p)) return 0;
                if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
                if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out var ds)) return ds;
                return 0;
            }

            int GetInt(string name, int fallback = 0)
            {
                if (!root.TryGetProperty(name, out var p)) return fallback;
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var si)) return si;
                return fallback;
            }

            string productType = type switch
            {
                "motorbike" => "Xe máy",
                "car" => "Ô tô",
                "accessory" => "Phụ kiện",
                "store" => "Showroom",
                _ => "Xe máy"
            };

            string unit = type switch
            {
                "accessory" => "món",
                "store" => "chi nhánh",
                _ => "chiếc"
            };

            string pathImage = GetString("image") ?? "";
            if (!string.IsNullOrWhiteSpace(pathImage) && !pathImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                if (!pathImage.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                    pathImage = "/images/" + pathImage.TrimStart('/');
            }

            return new AdminProduct
            {
                ProductID = displayId,
                ProductName = GetString("name") ?? "",
                Price = GetDecimal("price"),
                PathImage = pathImage,
                ProductType = productType,
                CategoryGroup = GetString("category") ?? (type == "store" ? "Showroom" : ""),
                Description = type == "store"
                    ? (GetString("address") ?? "")
                    : (GetString("category") ?? ""),
                Quantity = type == "store" ? 1 : GetInt("quantity", 1),
                Unit = unit,
                CreateDate = null,
                UpdateDate = null
            };
        }
    }
}