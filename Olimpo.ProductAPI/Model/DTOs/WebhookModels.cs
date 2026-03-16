namespace Olimpo.ProductAPI.Model.DTOs
{
    public class MercadoPagoNotification
    {
        public string Type { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public NotificationData? Data { get; set; }
    }

    public class NotificationData
    {
        public string Id { get; set; } = string.Empty;
    }
}
