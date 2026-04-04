using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(long id);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task<bool> DeleteAsync(Product product);
        Task<bool> ExistsAsync(long id);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(long categoryId);
        Task<ProductVariant?> GetVariantByIdAsync(long variantId);
        Task<ProductVariant?> GetVariantByProductAndSizeAsync(long productId, string size);
        Task UpdateVariantAsync(ProductVariant variant);

    }
}
