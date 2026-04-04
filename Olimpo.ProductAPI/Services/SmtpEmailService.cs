using System.Net;
using System.Net.Mail;
using System.Text;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOrderApprovedNotificationAsync(Order order)
        {
            var notificationTo = _configuration["EmailSettings:NotificationTo"];
            var host = _configuration["EmailSettings:SmtpHost"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("Configuração de email incompleta. Email de notificação não enviado.");
                return;
            }

            var portValue = _configuration["EmailSettings:SmtpPort"];
            var sslValue = _configuration["EmailSettings:EnableSsl"];

            var smtpPort = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
            var enableSsl = bool.TryParse(sslValue, out var parsedSsl) ? parsedSsl : true;

            using var smtpClient = new SmtpClient(host, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            // 1) Email administrativo
            if (!string.IsNullOrWhiteSpace(notificationTo))
            {
                var adminSubject = $"[OLIMPO] Pagamento aprovado - Pedido #{order.Id}";
                var adminBody = BuildAdminEmailBody(order);
                await SendEmailAsync(smtpClient, senderEmail, senderName, notificationTo, adminSubject, adminBody);
                _logger.LogInformation("Email administrativo enviado para {EmailDestino}, pedido {OrderId}", notificationTo, order.Id);
            }

            // 2) Email do cliente (mensagem clássica)
            if (!string.IsNullOrWhiteSpace(order.Email))
            {
                var customerSubject = $"Seu pedido #{order.Id} foi confirmado - OLIMPO";
                var customerBody = BuildCustomerEmailBody(order);
                await SendEmailAsync(smtpClient, senderEmail, senderName, order.Email, customerSubject, customerBody);
                _logger.LogInformation("Email de confirmação enviado para cliente {ClientEmail}, pedido {OrderId}", order.Email, order.Id);
            }
        }

        private static async Task SendEmailAsync(
            SmtpClient smtpClient,
            string senderEmail,
            string? senderName,
            string to,
            string subject,
            string body)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, string.IsNullOrWhiteSpace(senderName) ? "Olimpo" : senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(to);
            await smtpClient.SendMailAsync(message);
        }

        private static string BuildAdminEmailBody(Order order)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Pagamento aprovado!");
            sb.AppendLine();
            sb.AppendLine($"Pedido: #{order.Id}");
            sb.AppendLine($"Cliente: {order.NomeCompleto}");
            sb.AppendLine($"Email do cliente: {order.Email}");
            sb.AppendLine($"Telefone do cliente: {order.Telefone}");
            sb.AppendLine($"Valor total: R$ {order.Total:F2}");
            sb.AppendLine();
            sb.AppendLine("Itens:");

            foreach (var item in order.OrderItems)
            {
                sb.AppendLine($"- {item.ProductName} | Qtd: {item.Quantity} | Unit: R$ {item.UnitPrice:F2} | Total: R$ {item.TotalPrice:F2}");
            }

            sb.AppendLine();

            var hasDeliveryAddress =
                !string.IsNullOrWhiteSpace(order.Cep) &&
                !string.IsNullOrWhiteSpace(order.Rua) &&
                !string.IsNullOrWhiteSpace(order.Numero) &&
                !string.IsNullOrWhiteSpace(order.Bairro) &&
                !string.IsNullOrWhiteSpace(order.Cidade) &&
                !string.IsNullOrWhiteSpace(order.Estado);

            if (hasDeliveryAddress)
            {
                sb.AppendLine("Entrega: Sim");
                sb.AppendLine($"Endereço: {order.Rua}, {order.Numero}");
                sb.AppendLine($"Bairro: {order.Bairro}");
                sb.AppendLine($"Cidade/UF: {order.Cidade} - {order.Estado}");
                sb.AppendLine($"CEP: {order.Cep}");

                if (!string.IsNullOrWhiteSpace(order.Complemento))
                {
                    sb.AppendLine($"Complemento: {order.Complemento}");
                }
            }
            else
            {
                sb.AppendLine("Entrega: Não informada (possível retirada)");
            }

            return sb.ToString();
        }

        private static string BuildCustomerEmailBody(Order order)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Olá, {order.NomeCompleto}!");
            sb.AppendLine();
            sb.AppendLine("Recebemos seu pagamento e seu pedido foi confirmado com sucesso.");
            sb.AppendLine("Obrigado por comprar com a OLIMPO.");
            sb.AppendLine();
            sb.AppendLine($"Pedido: #{order.Id}");
            sb.AppendLine($"Total pago: R$ {order.Total:F2}");
            sb.AppendLine();
            sb.AppendLine("Resumo da compra:");

            foreach (var item in order.OrderItems)
            {
                var sizeInfo = string.IsNullOrWhiteSpace(item.Size) ? string.Empty : $" | Tam: {item.Size}";
                sb.AppendLine($"- {item.ProductName}{sizeInfo} | Qtd: {item.Quantity} | R$ {item.TotalPrice:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("Assim que houver atualização de envio, você será informado.");
            sb.AppendLine();
            sb.AppendLine("Atenciosamente,");
            sb.AppendLine("Equipe OLIMPO");

            return sb.ToString();
        }
    }
}
