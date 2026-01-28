using Microsoft.AspNetCore.Mvc;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;
using Olimpo.ProductAPI.Services;

namespace Olimpo.ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IOrderRepository orderRepository, IMercadoPagoService mercadoPagoService, ILogger<WebhookController> logger)
        {
            _orderRepository = orderRepository;
            _mercadoPagoService = mercadoPagoService;
            _logger = logger;
        }

        [HttpPost("mercadopago")]
        public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoNotification notification)
        {
            try
            {
                _logger.LogInformation($"Webhook recebido: {notification.Type} - {notification.Data?.Id}");

                // Mercado Pago envia notification.type = "payment"
                if (notification.Type != "payment")
                    return Ok(); // Ignora outros tipos

                if (string.IsNullOrEmpty(notification.Data?.Id))
                    return BadRequest("Payment ID não encontrado");

                // Buscar informações do pagamento
                var paymentInfo = await _mercadoPagoService.GetPaymentInfoAsync(notification.Data.Id);

                // Buscar pedido pelo ExternalReference (se você salvou)
                // Ou você pode buscar pelo MercadoPagoId
                var order = await _orderRepository.GetByMercadoPagoIdAsync(paymentInfo.PaymentId);

                if (order == null)
                {
                    _logger.LogWarning($"Pedido não encontrado para payment: {paymentInfo.PaymentId}");
                    return NotFound();
                }

                // Atualizar status baseado no status do pagamento
                OrderStatus newStatus = paymentInfo.Status switch
                {
                    "approved" => OrderStatus.Pago,
                    "rejected" => OrderStatus.Rejeitado,
                    "cancelled" => OrderStatus.Cancelado,
                    "pending" or "in_process" => OrderStatus.Pendente,
                    _ => order.Status // Mantém status atual
                };

                // Atualizar pedido
                order.MercadoPagoPaymentStatus = paymentInfo.Status;
                await _orderRepository.UpdateStatusAsync(order.Id, newStatus);

                _logger.LogInformation($"Pedido {order.Id} atualizado para status: {newStatus}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro no webhook: {ex.Message}");
                return StatusCode(500);
            }
        }

        public class MercadoPagoNotification
        {
            public string Type { get; set; } = string.Empty;
            public NotificationData? Data { get; set; }
        }

        public class NotificationData
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
