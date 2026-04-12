namespace VinfastWeb.Models
{
    public class AdminProduct
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string PathImage { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string CategoryGroup { get; set; } = "";
        public int Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public string Description { get; set; } = "";
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string FormattedPrice => Price.ToString("N0") + " đ";

        public string ImageUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PathImage))
                    return "/images/no-image.png";

                var path = PathImage.Replace("\\", "/").Trim();

                // Nếu đã là URL đầy đủ
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    return path;

                // Nếu đã có /images/
                if (path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                    return path;

                // 👉 QUAN TRỌNG: thêm /images vào đây
                return "/images/" + path.TrimStart('/');
            }
        }
    }
}