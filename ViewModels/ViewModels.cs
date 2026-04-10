using VinfastWeb.Models;

namespace VinfastWeb.ViewModels
{
    public class HomeViewModel
    {
        public List<Motorbike> FeaturedMotorbikes { get; set; } = new();
        public List<Car> FeaturedCars { get; set; } = new();
        public List<Store> Stores { get; set; } = new();
        public List<Voucher> Vouchers { get; set; } = new();
    }

    public class ProductsViewModel
    {
        public List<Motorbike> Motorbikes { get; set; } = new();
        public string? Category { get; set; }
        public string CategoryLabel { get; set; } = "";
    }

    public class CarsViewModel
    {
        public List<Car> Cars { get; set; } = new();
        public string? Category { get; set; }
        public string CategoryLabel { get; set; } = "";
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public long GrandTotal => Items.Sum(i => i.Total);
        public string FormattedGrandTotal => GrandTotal.ToString("N0").Replace(",", ".") + "đ";
    }

    public class SearchViewModel
    {
        public string Query { get; set; } = "";
        public List<Motorbike> Motorbikes { get; set; } = new();
        public List<Car> Cars { get; set; } = new();
        public List<Accessory> Accessories { get; set; } = new();
        public int TotalResults => Motorbikes.Count + Cars.Count + Accessories.Count;
    }

    public class LoginViewModel
    {
        public string Account { get; set; } = "";
        public string Password { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        public string Account { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }
}