using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;

namespace Olimpo.ProductAPI.Services
{
    public class StockReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockReservationCleanupService> _logger;

        public StockReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<StockReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock Reservation Cleanup Service iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredReservations();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao limpar reservas: {ex.Message}");
                }

                // Executar a cada 5 minutos
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CleanupExpiredReservations()
        {
            using var scope = _serviceProvider.CreateScope();
            var reservationRepository = scope.ServiceProvider.GetRequiredService<IStockReservationRepository>();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            var expiredReservations = await reservationRepository.GetExpiredReservationsAsync();

            foreach (var reservation in expiredReservations)
            {
                _logger.LogInformation($"Liberando reserva expirada: Order {reservation.OrderId}, Product {reservation.ProductId}");

                // Liberar reserva
                await reservationRepository.ReleaseReservationAsync(reservation.Id);

                // Cancelar pedido se ainda estiver pendente
                var order = await orderRepository.GetByIdAsync(reservation.OrderId);
                if (order != null && order.Status == OrderStatus.Pendente)
                {
                    await orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Cancelado);
                    _logger.LogInformation($"Pedido {order.Id} cancelado por expiração");
                }
            }

            if (expiredReservations.Any())
            {
                _logger.LogInformation($"{expiredReservations.Count()} reservas expiradas liberadas");
            }
        }
    }
}
