using VinfastWeb.Models;

namespace VinfastWeb.ViewModels
{
    public class AdminProductListViewModel
    {
        public List<AdminProduct> Items { get; set; } = new();
        public string Keyword { get; set; } = "";
        public string ProductType { get; set; } = "";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 1;
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
    }
}