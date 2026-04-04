namespace Olimpo.ProductAPI.Model.Entities
{
    public class StockReservation : BaseEntity
    {
        public long ProductId { get; set; }
        public long? ProductVariantId { get; set; }
        public long OrderId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsReleased { get; set; } = false;

        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
        public virtual Order Order { get; set; } = null!;

    }
}
