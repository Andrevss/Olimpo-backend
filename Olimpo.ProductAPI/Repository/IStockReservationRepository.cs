using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public interface IStockReservationRepository
    {
        Task<StockReservation> CreateAsync(StockReservation reservation);
        Task<IEnumerable<StockReservation>> GetByOrderIdAsync(long orderId);
        Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync();
        Task ReleaseReservationAsync(long reservationId);
        Task ReleaseOrderReservationsAsync(long orderId);
    }
}
