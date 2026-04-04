using Microsoft.AspNetCore.Authentication.Cookies;
using VinfastWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api-images/{**path}", async (string path, IConfiguration config, IHttpClientFactory httpFactory, HttpContext ctx) =>
{
    var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    var http = httpFactory.CreateClient();
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