using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VinfastWeb.Models;
using VinfastWeb.ViewModels;

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
            return await GetAsync<Motorbike>($"{_baseUrl}/api/products/motorbike/{id}");
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
                var (type, rawId) = DecodeAdminProductId(id);

                var res = await _http.GetStringAsync($"{_baseUrl}/api/admin/products/{type}/{rawId}");
                var doc = JsonDocument.Parse(res);
                var root = doc.RootElement;

                string productType = type switch
                {
                    "motorbike" => "Xe máy",
                    "car" => "Ô tô",
                    "accessory" => "Phụ kiện",
                    "store" => "Showroom",
                    _ => "Xe máy"
                };

                int productId = type switch
                {
                    "car" => 100000 + root.GetProperty("id").GetInt32(),
                    "accessory" => 200000 + root.GetProperty("id").GetInt32(),
                    "store" => 300000 + root.GetProperty("id").GetInt32(),
                    _ => root.GetProperty("id").GetInt32()
                };

                return new AdminProduct
                {
                    ProductID = productId,
                    ProductName = root.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                    Price = root.TryGetProperty("price", out var price) && price.ValueKind != JsonValueKind.Null ? price.GetDecimal() : 0,
                    PathImage = root.TryGetProperty("image", out var image) ? image.GetString() ?? "" : "",
                    ProductType = productType,
                    CategoryGroup = root.TryGetProperty("category", out var category) ? category.GetString() ?? "" : "",
                    Description = root.TryGetProperty("address", out var address)
                        ? address.GetString() ?? ""
                        : (root.TryGetProperty("category", out var cat2) ? cat2.GetString() ?? "" : ""),
                    Quantity = 1,
                    Unit = type == "store" ? "chi nhánh" : "chiếc",
                    CreateDate = DateTime.Today,
                    UpdateDate = DateTime.Today
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAdminProductAsync {Id} error", id);
                return null;
            }
        }

        public async Task<(bool success, string? message)> CreateAdminProductAsync(AdminProduct model, string? token)
        {
            object payload;

            switch ((model.ProductType ?? "").Trim().ToLower())
            {
                case "motorbike":
                    payload = new
                    {
                        productType = "motorbike",
                        name = model.ProductName,
                        price = model.Price,
                        image = model.PathImage ?? "",
                        category = model.CategoryGroup ?? "",
                        is_featured = 1
                    };
                    break;

                case "car":
                    payload = new
                    {
                        productType = "car",
                        name = model.ProductName,
                        price = model.Price,
                        image = model.PathImage ?? "",
                        category = model.CategoryGroup ?? "dong_co_dien"
                    };
                    break;

                case "accessory":
                    payload = new
                    {
                        productType = "accessory",
                        name = model.ProductName,
                        price = model.Price,
                        image = model.PathImage ?? "",
                        category = model.CategoryGroup ?? "phu_kien",
                        is_featured = 1
                    };
                    break;

                case "store":
                    payload = new
                    {
                        productType = "store",
                        name = model.ProductName,
                        rating = "5.0",
                        address = model.Description ?? "Địa chỉ đang cập nhật",
                        image = model.PathImage ?? "",
                        route = ""
                    };
                    break;

                default:
                    return (false, "Loại sản phẩm không hợp lệ.");
            }

            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products",
                token,
                payload
            );

            using (req)
            {
                return await SendWithMessageAsync(req);
            }
        }

        public async Task<(bool success, string? message)> UpdateAdminProductAsync(AdminProduct model, string? token)
        {
            var type = GetAdminType(model);
            var (_, rawId) = DecodeAdminProductId(model.ProductID);

            object payload = type switch
            {
                "motorbike" => new
                {
                    name = model.ProductName,
                    price = model.Price,
                    image = model.PathImage ?? "",
                    category = model.CategoryGroup ?? "",
                    is_featured = 1
                },
                "car" => new
                {
                    name = model.ProductName,
                    price = model.Price,
                    image = model.PathImage ?? "",
                    category = model.CategoryGroup ?? "dong_co_dien"
                },
                "accessory" => new
                {
                    name = model.ProductName,
                    price = model.Price,
                    image = model.PathImage ?? "",
                    category = model.CategoryGroup ?? "phu_kien",
                    is_featured = 1
                },
                "store" => new
                {
                    name = model.ProductName,
                    rating = "5.0",
                    address = model.Description ?? "Địa chỉ đang cập nhật",
                    image = model.PathImage ?? "",
                    route = ""
                },
                _ => new
                {
                    name = model.ProductName,
                    price = model.Price,
                    image = model.PathImage ?? "",
                    category = model.CategoryGroup ?? ""
                }
            };

            var req = CreateRequest(
                HttpMethod.Put,
                $"{_baseUrl}/api/admin/products/{type}/{rawId}",
                token,
                payload
            );

            using (req)
            {
                return await SendWithMessageAsync(req);
            }
        }

        public async Task<(bool success, string? message)> DeleteAdminProductAsync(int id, string? token)
        {
            var (type, rawId) = DecodeAdminProductId(id);

            var req = CreateRequest(
                HttpMethod.Delete,
                $"{_baseUrl}/api/admin/products/{type}/{rawId}",
                token
            );

            using (req)
            {
                return await SendWithMessageAsync(req);
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

        public async Task<(bool success, string? message)> DuplicateAdminProductAsync(int id, string? token)
        {
            var (type, rawId) = DecodeAdminProductId(id);

            var req = CreateRequest(
                HttpMethod.Post,
                $"{_baseUrl}/api/admin/products/duplicate/{type}/{rawId}",
                token
            );

            using (req)
            {
                return await SendWithMessageAsync(req);
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
        public async Task<SearchViewModel> SearchAllAsync(string keyword)
        {
            // 1. Nếu từ khóa trống, trả về Model rỗng ngay lập tức
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new SearchViewModel { Query = "" };
            }

            try
            {
                // 2. Gọi API đến Backend Node.js
                // Lưu ý: Đường dẫn "/api/search" phải khớp với route mà bạn Backend đã đặt
                var response = await _http.GetFromJsonAsync<SearchViewModel>(
                    $"{_baseUrl}/api/search?q={Uri.EscapeDataString(keyword)}"
                );

                // 3. Trả về kết quả, nếu null thì trả về Model mới để tránh lỗi giao diện
                if (response != null)
                {
                    response.Query = keyword; // Gán lại từ khóa để hiển thị trên View
                    return response;
                }

                return new SearchViewModel { Query = keyword };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần và trả về model rỗng để web không bị "văng"
                Console.WriteLine($"Lỗi tìm kiếm: {ex.Message}");
                return new SearchViewModel { Query = keyword };
            }
        }
        private string GetAdminType(AdminProduct model)
        {
            var t = (model.ProductType ?? "").Trim().ToLower();

            return t switch
            {
                "motorbike" => "motorbike",
                "xe máy" => "motorbike",
                "car" => "car",
                "ô tô" => "car",
                "accessory" => "accessory",
                "phụ kiện" => "accessory",
                "store" => "store",
                "showroom" => "store",
                _ => "motorbike"
            };
        }

        private (string type, int rawId) DecodeAdminProductId(int productId)
        {
            if (productId >= 300000) return ("store", productId - 300000);
            if (productId >= 200000) return ("accessory", productId - 200000);
            if (productId >= 100000) return ("car", productId - 100000);
            return ("motorbike", productId);
        }
    }
}