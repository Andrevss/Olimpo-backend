using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Context;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MySQLContext _context;
        public CategoryRepository(MySQLContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
        }
        public async Task<Category?> GetByIdAsync(long id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }
        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }
        public async Task UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(Category category)
        {
            category.IsActive = false;
            _context.Categories.Update(category);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }
        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == id && c.IsActive);
        }
    }
}
