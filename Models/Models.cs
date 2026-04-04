namespace VinfastWeb.Models
{
    public class Motorbike
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public long Price { get; set; }
        public string Image { get; set; } = "";
        public string Category { get; set; } = "";

        public string FormattedPrice =>
            Price.ToString("N0").Replace(",", ".") + "đ";

        public string ImageUrl => $"/api-images/motorbike/{System.IO.Path.GetFileName(Image)}";
    }

    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public long Price { get; set; }
        public string Image { get; set; } = "";
        public string Category { get; set; } = "";

        public string FormattedPrice =>
            Price.ToString("N0").Replace(",", ".") + "đ";

        public string ImageUrl => $"/api-images/car/{System.IO.Path.GetFileName(Image)}";
    }

    public class Accessory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public long Price { get; set; }
        public string Image { get; set; } = "";
        public string Category { get; set; } = "";

        public string FormattedPrice =>
            Price.ToString("N0").Replace(",", ".") + "đ";

        public string ImageUrl => $"/api-images/accessory/{System.IO.Path.GetFileName(Image)}";
    }

    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Rating { get; set; }
        public string Address { get; set; } = "";
        public string Image { get; set; } = "";
        public string Route { get; set; } = "";

        public string ImageUrl => $"/api-images/store/{System.IO.Path.GetFileName(Image)}";
    }

    public class Voucher
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Code { get; set; } = "";
        public string Image { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public string ImageUrl => $"/api-images/voucher/{System.IO.Path.GetFileName(Image)}";
    }

    public class User
    {
        public int Id { get; set; }
        public string Account { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductType { get; set; } = ""; // motorbike / car / accessory
        public string Name { get; set; } = "";
        public long Price { get; set; }
        public string Image { get; set; } = "";
        public int Quantity { get; set; }

        public long Total => Price * Quantity;
        public string FormattedPrice => Price.ToString("N0").Replace(",", ".") + "đ";
        public string FormattedTotal => Total.ToString("N0").Replace(",", ".") + "đ";
    }
}