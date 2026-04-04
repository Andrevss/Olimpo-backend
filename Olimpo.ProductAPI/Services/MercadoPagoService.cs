using Olimpo.ProductAPI.Model.Entities;
using System.Text;
using System.Text.Json;

namespace Olimpo.ProductAPI.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _successUrl;
        private readonly string _failureUrl;
        private readonly string _pendingUrl;
        private readonly bool _useSandbox;

        public MercadoPagoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _accessToken = _configuration["MercadoPago:AccessToken"]!;
            _successUrl = _configuration["MercadoPago:SuccessUrl"]!;
            _failureUrl = _configuration["MercadoPago:FailureUrl"]!;
            _pendingUrl = _configuration["MercadoPago:PendingUrl"]!;
            _useSandbox = bool.TryParse(_configuration["MercadoPago:UseSandbox"], out var useSandbox) && useSandbox;
        }

        public async Task<string> CreatePreferenceAsync(Order order)
        {
            try
            {
                var validItems = order.OrderItems
                    .Where(item => item.Quantity > 0 && item.UnitPrice > 0 && !string.IsNullOrWhiteSpace(item.ProductName))
                    .Select(item => new
                    {
                        title = string.IsNullOrWhiteSpace(item.Size) ? item.ProductName : $"{item.ProductName} - {item.Size}",
                        quantity = item.Quantity,
                        currency_id = "BRL",
                        unit_price = item.UnitPrice
                    })
                    .ToArray();

                if (!validItems.Any())
                {
                    throw new Exception("Pedido sem itens válidos para criar preferência (quantity > 0, unit_price > 0 e title obrigatório).");
                }

                // Criar objeto de preferência
                var preference = new
                {
                    items = validItems,

                    payer = new
                    {
                        name = order.NomeCompleto,
                        email = order.Email
                    },

                    back_urls = new
                    {
                        success = $"{_successUrl}?orderId={order.Id}",
                        failure = $"{_failureUrl}?orderId={order.Id}",
                        pending = $"{_pendingUrl}?orderId={order.Id}"
                    },

                    auto_return = "approved",
                    notification_url = $"{_configuration["AppUrl"]}/api/webhooks/mercadopago",
                    external_reference = order.Id.ToString(),
                    statement_descriptor = "OLIMPO",
                    expires = true,
                    expiration_date_from = DateTime.UtcNow.ToString("o"),
                    expiration_date_to = DateTime.UtcNow.AddDays(7).ToString("o")
                };

                // Serializar para JSON
                var json = JsonSerializer.Serialize(preference, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Fazer requisição para API do Mercado Pago
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/checkout/preferences")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Status {(int)response.StatusCode} ({response.StatusCode}). Detalhes: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Retornar URL de checkout
                // Para produção: init_point
                // Para testes: sandbox_init_point
                var pointProperty = _useSandbox ? "sandbox_init_point" : "init_point";
                if (!result.TryGetProperty(pointProperty, out var checkoutUrlProperty))
                {
                    throw new Exception($"Campo '{pointProperty}' não encontrado na resposta do Mercado Pago: {responseContent}");
                }

                return checkoutUrlProperty.GetString()!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar preferência no Mercado Pago: {ex.Message}", ex);
            }
        }

        public async Task<MercadoPagoPaymentInfo> GetPaymentInfoAsync(string paymentId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{paymentId}");
                request.Headers.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var payment = JsonSerializer.Deserialize<JsonElement>(responseContent);

                return new MercadoPagoPaymentInfo
                {
                    Status = payment.GetProperty("status").GetString()!,
                    PaymentId = payment.GetProperty("id").GetInt64().ToString(),
                    Amount = payment.GetProperty("transaction_amount").GetDecimal(),
                    ExternalReference = payment.TryGetProperty("external_reference", out var externalReference)
                        ? externalReference.GetString()
                        : null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar pagamento: {ex.Message}", ex);
            }
        }
    }
}
