namespace Olimpo.ProductAPI.Model.Entities
{
    public class ProductVariant : BaseEntity
    {
        public long ProductId { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int ReservedStock { get; set; }

        public virtual Product Product { get; set; } = null!;

        public int AvailableStock => Stock - ReservedStock;
    }
}
