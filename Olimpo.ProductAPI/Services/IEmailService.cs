using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Services
{
    public interface IEmailService
    {
        Task SendOrderApprovedNotificationAsync(Order order);
    }
}
