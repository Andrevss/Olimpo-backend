namespace Olimpo.ProductAPI.Model.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Relacionamento: Uma categoria tem vários produtos
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
