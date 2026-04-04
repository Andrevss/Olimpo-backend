namespace Olimpo.ProductAPI.Model.DTOs
{
    public class ProductDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<ProductVariantDTO> Variants { get; set; } = new();
    }

    public class CreateProductDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public List<CreateProductVariantDTO> Variants { get; set; } = new();
    }

    public class UpdateProductDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long CategoryId { get; set; }
        public List<CreateProductVariantDTO> Variants { get; set; } = new();
    }

    public class ProductVariantDTO
    {
        public long Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock { get; set; }
    }

    public class CreateProductVariantDTO
    {
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
