namespace Olimpo.ProductAPI.Model.Entities
{
    public class OrderItem : BaseEntity
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public long? ProductVariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string? Size { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual Order Order { get; set; } = null;
        public virtual Product Product { get; set; } = null;
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}
