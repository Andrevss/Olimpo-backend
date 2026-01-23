using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(long id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task<bool> DeleteAsync(Category category);
        Task<bool> ExistsAsync(long id);
    }
}
