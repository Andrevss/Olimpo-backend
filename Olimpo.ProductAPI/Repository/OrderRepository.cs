using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace Olimpo.ProductAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly MySQLContext _context;

        public OrderRepository(MySQLContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByMercadoPagoIdAsync(string mercadoPagoId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.MercadoPagoId == mercadoPagoId);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(order.Id))!;
        }

        public async Task<Order> UpdateStatusAsync(long id, OrderStatus status)
        {
            var order = await GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Pedido não encontrado");
            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }
    }
}
