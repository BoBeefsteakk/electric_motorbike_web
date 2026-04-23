using Microsoft.AspNetCore.Authentication.Cookies;
using VinfastWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// HttpClient cho ApiService - cho phep goi HTTP tu HTTPS app
builder.Services.AddHttpClient<ApiService>().ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// HttpClient chung cho proxy anh
builder.Services.AddHttpClient("proxy").ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddHttpClient();


var app = builder.Build();

app.UseDeveloperExceptionPage();
// app.UseHttpsRedirection(); // Tat khi dev

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Proxy anh tu Node.js backend
app.MapGet("/api-images/{**path}", async (string path, IConfiguration config, IHttpClientFactory httpFactory) =>
{
    var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    var http = httpFactory.CreateClient("proxy");
    try
    {
        var bytes = await http.GetByteArrayAsync($"{baseUrl}/images/{path}");
        var ext = System.IO.Path.GetExtension(path).ToLower();
        var mime = ext switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" };
        return Results.File(bytes, mime);
    }
    catch { return Results.NotFound(); }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();