namespace Projet__E_commerce.Models
{
    public class RegionViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Coops { get; set; }
        public int Percent { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string Image { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Cooperative { get; set; } = string.Empty;
        public bool IsBestSeller { get; set; }
        public bool IsNew { get; set; }
    }

    public class OrderViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Items { get; set; }
        public string StatusClass { get; set; } = string.Empty;
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    public class ActivityViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
