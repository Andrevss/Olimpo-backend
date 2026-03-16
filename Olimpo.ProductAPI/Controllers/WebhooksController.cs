using Microsoft.AspNetCore.Mvc;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;
using Olimpo.ProductAPI.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Olimpo.ProductAPI.Model.DTOs;

namespace Olimpo.ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockReservationRepository _stockReservationRepository;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IStockReservationRepository stockReservationRepository,
            IMercadoPagoService mercadoPagoService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<WebhooksController> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stockReservationRepository = stockReservationRepository;
            _mercadoPagoService = mercadoPagoService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/webhooks/mercadopago
        [HttpPost("mercadopago")]
        public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoNotification notification)
        {
            try
            {
                _logger.LogInformation($"📨 Webhook recebido: Type={notification.Type}, Action={notification.Action}, PaymentId={notification.Data?.Id}");

                // Mercado Pago envia diferentes tipos de notificação
                if (notification.Type != "payment")
                {
                    _logger.LogInformation($"⏭️ Tipo de notificação ignorado: {notification.Type}");
                    return Ok();
                }

                if (string.IsNullOrEmpty(notification.Data?.Id))
                {
                    _logger.LogWarning("⚠️ Payment ID não encontrado na notificação");
                    return BadRequest("Payment ID não encontrado");
                }

                if (!IsValidWebhookSignature(notification.Data.Id))
                {
                    _logger.LogWarning("⚠️ Assinatura de webhook inválida para payment {PaymentId}", notification.Data.Id);
                    return Unauthorized();
                }

                // Buscar informações do pagamento no Mercado Pago
                var paymentInfo = await _mercadoPagoService.GetPaymentInfoAsync(notification.Data.Id);

                if (paymentInfo == null)
                {
                    _logger.LogWarning("⚠️ Informações de pagamento não encontradas para payment {PaymentId}", notification.Data.Id);
                    return NotFound();
                }

                _logger.LogInformation($"💳 Pagamento {paymentInfo.PaymentId}: Status={paymentInfo.Status}, Amount={paymentInfo.Amount}");

                // Tentar obter external_reference a partir do objeto paymentInfo (preenchido pelo serviço)
                string? externalReferenceString = paymentInfo?.ExternalReference;

                JsonElement? payment = null;

                if (string.IsNullOrEmpty(externalReferenceString))
                {
                    // Se não tiver externalReference no paymentInfo, buscar detalhes diretamente na API do MercadoPago
                    payment = await GetPaymentDetailsAsync(notification.Data.Id);
                    if (!payment.HasValue || !payment.Value.TryGetProperty("external_reference", out var externalRef))
                    {
                        _logger.LogWarning($"⚠️ External reference não encontrado para payment {notification.Data.Id}");
                        return NotFound();
                    }

                    externalReferenceString = externalRef.GetString();
                }

                if (string.IsNullOrEmpty(externalReferenceString))
                {
                    _logger.LogWarning($"⚠️ External reference não encontrado para payment {notification.Data.Id}");
                    return NotFound();
                }

                if (!long.TryParse(externalReferenceString, out var orderId))
                {
                    _logger.LogWarning($"⚠️ External reference inválido: {externalReferenceString}");
                    return NotFound();
                }

                var order = await _orderRepository.GetByIdAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning($"⚠️ Pedido {orderId} não encontrado");
                    return NotFound();
                }

                _logger.LogInformation($"📦 Processando pedido {order.Id}: Status atual={order.Status}");

                // Processar baseado no status do pagamento
                await ProcessPaymentStatusAsync(order, paymentInfo.Status);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erro no webhook: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private bool IsValidWebhookSignature(string dataId)
        {
            var secret = _configuration["MercadoPago:WebhookSecret"];
            var shouldValidate = bool.TryParse(_configuration["MercadoPago:ValidateWebhookSignature"], out var validateFromConfig)
                ? validateFromConfig
                : !string.IsNullOrWhiteSpace(secret);

            if (!shouldValidate)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            if (!Request.Headers.TryGetValue("x-signature", out var signatureHeaderValues))
            {
                return false;
            }

            if (!Request.Headers.TryGetValue("x-request-id", out var requestIdHeaderValues))
            {
                return false;
            }

            var signatureHeader = signatureHeaderValues.ToString();
            var requestId = requestIdHeaderValues.ToString();

            if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(requestId))
            {
                return false;
            }

            var ts = GetSignaturePart(signatureHeader, "ts");
            var receivedV1 = GetSignaturePart(signatureHeader, "v1");

            if (string.IsNullOrWhiteSpace(ts) || string.IsNullOrWhiteSpace(receivedV1))
            {
                return false;
            }

            var manifest = $"id:{dataId};request-id:{requestId};ts:{ts};";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
            var expectedV1 = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return string.Equals(expectedV1, receivedV1, StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetSignaturePart(string signatureHeader, string key)
        {
            var parts = signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var index = part.IndexOf('=');
                if (index <= 0)
                {
                    continue;
                }

                var partKey = part[..index].Trim();
                var partValue = part[(index + 1)..].Trim();

                if (string.Equals(partKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return partValue;
                }
            }

            return null;
        }

        private async Task ProcessPaymentStatusAsync(Order order, string paymentStatus)
        {
            switch (paymentStatus.ToLower())
            {
                case "approved": // ✅ PAGAMENTO APROVADO
                    await HandleApprovedPayment(order);
                    break;

                case "rejected": // ❌ PAGAMENTO RECUSADO
                case "cancelled": // ❌ PAGAMENTO CANCELADO
                    await HandleRejectedOrCancelledPayment(order, paymentStatus);
                    break;

                case "pending": // ⏳ PAGAMENTO PENDENTE
                case "in_process": // ⏳ EM PROCESSAMENTO
                case "in_mediation": // ⏳ EM MEDIAÇÃO
                    await HandlePendingPayment(order, paymentStatus);
                    break;

                case "refunded": // 💰 REEMBOLSADO
                case "charged_back": // 💰 ESTORNADO
                    await HandleRefundedPayment(order, paymentStatus);
                    break;

                default:
                    _logger.LogWarning($"⚠️ Status de pagamento desconhecido: {paymentStatus}");
                    break;
            }
        }

        // ✅ PAGAMENTO APROVADO
        private async Task HandleApprovedPayment(Order order)
        {
            _logger.LogInformation($"✅ Pagamento APROVADO para pedido {order.Id}");

            if (order.Status == OrderStatus.Pago)
            {
                _logger.LogInformation($"⏭️ Pedido {order.Id} já está pago, ignorando");
                return;
            }

            // 1. Atualizar status do pedido
            order.MercadoPagoPaymentStatus = "approved";
            await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Pago);

            // 2. Confirmar estoque (diminuir Stock real e liberar reserva)
            var reservations = await _stockReservationRepository.GetByOrderIdAsync(order.Id);

            foreach (var reservation in reservations)
            {
                var product = await _productRepository.GetByIdAsync(reservation.ProductId);

                if (product != null)
                {
                    // Diminui estoque real
                    product.Stock -= reservation.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;

                    await _productRepository.UpdateAsync(product);

                    _logger.LogInformation($"📦 Produto {product.Name}: Stock={product.Stock}");
                }

                // Marcar reserva como liberada
                await _stockReservationRepository.ReleaseReservationAsync(reservation.Id);
            }

            _logger.LogInformation($"✅ Pedido {order.Id} confirmado e estoque atualizado");

            try
            {
                await _emailService.SendOrderApprovedNotificationAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de confirmação do pedido {OrderId}", order.Id);
            }
        }

        // ❌ PAGAMENTO RECUSADO/CANCELADO
        private async Task HandleRejectedOrCancelledPayment(Order order, string status)
        {
            _logger.LogInformation($"❌ Pagamento {status.ToUpper()} para pedido {order.Id}");

            // 1. Atualizar status do pedido
            var newStatus = status.ToLower() == "cancelled" ? OrderStatus.Cancelado : OrderStatus.Rejeitado;
            order.MercadoPagoPaymentStatus = status;
            await _orderRepository.UpdateStatusAsync(order.Id, newStatus);

            // 2. Liberar estoque reservado (devolver para disponível)
            var reservations = await _stockReservationRepository.GetByOrderIdAsync(order.Id);

            foreach (var reservation in reservations)
            {
                await _stockReservationRepository.ReleaseReservationAsync(reservation.Id);
            }

            _logger.LogInformation($"❌ Pedido {order.Id} {status} e estoque liberado");
        }

        // ⏳ PAGAMENTO PENDENTE
        private async Task HandlePendingPayment(Order order, string status)
        {
            _logger.LogInformation($"⏳ Pagamento PENDENTE para pedido {order.Id}: {status}");

            order.MercadoPagoPaymentStatus = status;

            // Mantém como Pendente, não mexe no estoque (reserva continua)
            if (order.Status != OrderStatus.Pendente)
            {
                await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Pendente);
            }
        }

        // 💰 REEMBOLSO/ESTORNO
        private async Task HandleRefundedPayment(Order order, string status)
        {
            _logger.LogInformation($"💰 Pagamento {status.ToUpper()} para pedido {order.Id}");

            // 1. Atualizar status
            order.MercadoPagoPaymentStatus = status;
            await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Cancelado);

            // 2. Devolver estoque (se já tinha sido diminuído)
            if (order.Status == OrderStatus.Pago || order.Status == OrderStatus.Enviado)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);

                    if (product != null)
                    {
                        product.Stock += item.Quantity; // Devolve ao estoque
                        product.UpdatedAt = DateTime.UtcNow;

                        await _productRepository.UpdateAsync(product);

                        _logger.LogInformation($"🔄 Estoque devolvido por reembolso - Produto {product.Name}: Stock={product.Stock}");
                    }
                }
            }

            _logger.LogInformation($"💰 Pedido {order.Id} reembolsado e estoque devolvido");
        }

        // Método auxiliar para buscar detalhes do pagamento
        private async Task<JsonElement?> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api.mercadopago.com/v1/payments/{paymentId}");

                var accessToken = _configuration["MercadoPago:AccessToken"];
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                using var httpClient = new HttpClient();
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<JsonElement>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar detalhes do pagamento: {ex.Message}");
                return null;
            }
        }
    }
}