using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Context;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Repository
{
    public class StockReservationRepository : IStockReservationRepository
    {
        private readonly MySQLContext _context;

        public StockReservationRepository(MySQLContext context)
        {
            _context = context;
        }

        public async Task<StockReservation> CreateAsync(StockReservation reservation)
        {
            _context.StockReservation.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<IEnumerable<StockReservation>> GetByOrderIdAsync(long orderId)
        {
            return await _context.StockReservation
                .Include(sr => sr.Product)
                .Include(sr => sr.ProductVariant)
                .Where(sr => sr.OrderId == orderId && !sr.IsReleased)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockReservation>> GetExpiredReservationsAsync()
        {
            return await _context.StockReservation
                .Include(sr => sr.Product)
                .Include(sr => sr.ProductVariant)
                .Include(sr => sr.Order)
                .Where(sr => !sr.IsReleased && sr.ExpireAt < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task ReleaseReservationAsync(long reservationId)
        {
            var reservation = await _context.StockReservation
                .Include(sr => sr.Product)
                .Include(sr => sr.ProductVariant)
                .FirstOrDefaultAsync(sr => sr.Id == reservationId);

            if (reservation != null && !reservation.IsReleased)
            {
                reservation.IsReleased = true;

                if (reservation.ProductVariant != null)
                {
                    reservation.ProductVariant.ReservedStock -= reservation.Quantity;
                    reservation.ProductVariant.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    reservation.Product.ReservedStock -= reservation.Quantity;
                    reservation.Product.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ReleaseOrderReservationsAsync(long orderId)
        {
            var reservations = await GetByOrderIdAsync(orderId);

            foreach (var reservation in reservations)
            {
                reservation.IsReleased = true;

                if (reservation.ProductVariant != null)
                {
                    reservation.ProductVariant.ReservedStock -= reservation.Quantity;
                    reservation.ProductVariant.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    reservation.Product.ReservedStock -= reservation.Quantity;
                    reservation.Product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
