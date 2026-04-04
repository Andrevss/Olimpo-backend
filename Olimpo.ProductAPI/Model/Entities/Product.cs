namespace Olimpo.ProductAPI.Model.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReservedStock { get; set; } = 0;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Chave estrangeira
        public long CategoryId { get; set; }

        // Relacionamento: Um produto pertence a uma categoria
        public virtual Category Category { get; set; } = null!;

        // Relacionamento: um produto pode ter variações por tamanho
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public int AvailableStock => Stock - ReservedStock;
    }
}
