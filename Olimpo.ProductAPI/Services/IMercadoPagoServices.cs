using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Services
{
    public interface IMercadoPagoService
    {
        Task<string> CreatePreferenceAsync(Order order);
        Task<MercadoPagoPaymentInfo> GetPaymentInfoAsync(string paymentId);
    }

    public class MercadoPagoPaymentInfo
    {
        public string Status { get; set; } = string.Empty; 
        public string PaymentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ExternalReference { get; set; }
    }
}
