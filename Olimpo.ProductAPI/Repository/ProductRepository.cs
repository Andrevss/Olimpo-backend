using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Context;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MySQLContext _context;


        public ProductRepository(MySQLContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(long id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(long categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Product product)
        {
            product.IsActive = false;
            _context.Products.Update(product);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id && p.IsActive);
        }
    }
}
