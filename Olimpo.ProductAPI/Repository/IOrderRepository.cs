using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(long id);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateStatusAsync(long id, OrderStatus status);
        Task<bool> ExistsAsync(long id);
        Task<Order> GetByMercadoPagoIdAsync(string mercadoPagoId);
    }
}
