// ============================================
// Tests/WebhookControllerTests.cs
// ============================================
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Olimpo.ProductAPI.Controllers;
using Olimpo.ProductAPI.Model.DTOs;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;
using Olimpo.ProductAPI.Services;
using Xunit;

namespace Olimpo.Tests
{
    public class WebhookControllerTests
    {
        // Mocks (objetos falsos que simulam os repositórios)
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IStockReservationRepository> _stockReservationRepositoryMock;
        private readonly Mock<IMercadoPagoService> _mercadoPagoServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<WebhooksController>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly WebhooksController _controller;

        public WebhookControllerTests()
        {
            // Criar mocks
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _stockReservationRepositoryMock = new Mock<IStockReservationRepository>();
            _mercadoPagoServiceMock = new Mock<IMercadoPagoService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<WebhooksController>>();
            _configurationMock = new Mock<IConfiguration>();

            // Criar controller com mocks
            _controller = new WebhooksController(
                _orderRepositoryMock.Object,
                _productRepositoryMock.Object,
                _stockReservationRepositoryMock.Object,
                _mercadoPagoServiceMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _loggerMock.Object
            );
        }

        // ============================================
        // TESTES DE PAGAMENTO APROVADO
        // ============================================

        [Fact]
        public async Task Webhook_PagamentoAprovado_DeveAtualizarStatusParaPago()
        {
            // ARRANGE (configurar cenário)
            var order = CreateFakeOrder(1, OrderStatus.Pendente);
            var product = CreateFakeProduct(1, stock: 10, reservedStock: 2);
            var reservation = CreateFakeReservation(1, orderId: 1, productId: 1, quantity: 2);

            var paymentInfo = new MercadoPagoPaymentInfo
            {
                Status = "approved",
                PaymentId = "123456",
                Amount = 199.80m,
                ExternalReference = "1"
            };

            // Configurar mocks para retornar dados falsos
            _mercadoPagoServiceMock
                .Setup(s => s.GetPaymentInfoAsync("123456"))
                .ReturnsAsync(paymentInfo);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);

            _stockReservationRepositoryMock
                .Setup(r => r.GetByOrderIdAsync(1))
                .ReturnsAsync(new List<StockReservation> { reservation });

            _stockReservationRepositoryMock
                .Setup(r => r.ReleaseReservationAsync(reservation.Id))
                .Callback(() =>
                {
                    if (!reservation.IsReleased)
                    {
                        reservation.IsReleased = true;
                        product.ReservedStock -= reservation.Quantity;
                    }
                })
                .Returns(Task.CompletedTask);

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(r => r.UpdateStatusAsync(1, OrderStatus.Pago))
                .ReturnsAsync(order);

            _emailServiceMock
                .Setup(s => s.SendOrderApprovedNotificationAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            var notification = new MercadoPagoNotification
            {
                Type = "payment",
                Data = new NotificationData { Id = "123456" }
            };

            // ACT (executar o webhook)
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT (verificar resultados)
            result.Should().BeOfType<OkResult>();

            // Verificar que atualizou status para Pago
            _orderRepositoryMock.Verify(
                r => r.UpdateStatusAsync(1, OrderStatus.Pago),
                Times.Once
            );

            // Verificar que diminuiu estoque
            _productRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Product>(p => p.Stock == 8)),
                Times.Once
            );

            // Verificar que liberou reserva
            _stockReservationRepositoryMock.Verify(
                r => r.ReleaseReservationAsync(reservation.Id),
                Times.Once
            );

            _emailServiceMock.Verify(
                s => s.SendOrderApprovedNotificationAsync(order),
                Times.Once
            );

            product.ReservedStock.Should().Be(0);
        }

        [Fact]
        public async Task Webhook_PagamentoAprovado_PedidoJaPago_DeveIgnorar()
        {
            // ARRANGE
            var order = CreateFakeOrder(1, OrderStatus.Pago); // Já está pago!

            var paymentInfo = new MercadoPagoPaymentInfo
            {
                Status = "approved",
                PaymentId = "123456",
                Amount = 199.80m,
                ExternalReference = "1"
            };

            _mercadoPagoServiceMock
                .Setup(s => s.GetPaymentInfoAsync("123456"))
                .ReturnsAsync(paymentInfo);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);

            _emailServiceMock
                .Setup(s => s.SendOrderApprovedNotificationAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            var notification = new MercadoPagoNotification
            {
                Type = "payment",
                Data = new NotificationData { Id = "123456" }
            };

            // ACT
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // NÃO deve atualizar status (já está pago)
            _orderRepositoryMock.Verify(
                r => r.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<OrderStatus>()),
                Times.Never
            );

            _emailServiceMock.Verify(
                s => s.SendOrderApprovedNotificationAsync(It.IsAny<Order>()),
                Times.Never
            );
        }

        // ============================================
        // TESTES DE PAGAMENTO RECUSADO
        // ============================================

        [Fact]
        public async Task Webhook_PagamentoRecusado_DeveLiberarEstoque()
        {
            // ARRANGE
            var order = CreateFakeOrder(1, OrderStatus.Pendente);
            var reservation = CreateFakeReservation(1, orderId: 1, productId: 1, quantity: 2);

            var paymentInfo = new MercadoPagoPaymentInfo
            {
                Status = "rejected",
                PaymentId = "123456",
                Amount = 199.80m,
                ExternalReference = "1"
            };

            _mercadoPagoServiceMock
                .Setup(s => s.GetPaymentInfoAsync("123456"))
                .ReturnsAsync(paymentInfo);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);

            _stockReservationRepositoryMock
                .Setup(r => r.GetByOrderIdAsync(1))
                .ReturnsAsync(new List<StockReservation> { reservation });

            _orderRepositoryMock
                .Setup(r => r.UpdateStatusAsync(1, OrderStatus.Rejeitado))
                .ReturnsAsync(order);

            var notification = new MercadoPagoNotification
            {
                Type = "payment",
                Data = new NotificationData { Id = "123456" }
            };

            // ACT
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // Verificar que atualizou status para Rejeitado
            _orderRepositoryMock.Verify(
                r => r.UpdateStatusAsync(1, OrderStatus.Rejeitado),
                Times.Once
            );

            // Verificar que liberou reserva
            _stockReservationRepositoryMock.Verify(
                r => r.ReleaseReservationAsync(reservation.Id),
                Times.Once
            );
        }

        // ============================================
        // TESTES DE NOTIFICAÇÕES IGNORADAS
        // ============================================

        [Fact]
        public async Task Webhook_TipoNaoEhPayment_DeveIgnorar()
        {
            // ARRANGE
            var notification = new MercadoPagoNotification
            {
                Type = "merchant_order", // Não é payment!
                Data = new NotificationData { Id = "123" }
            };

            // ACT
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // Não deve chamar nenhum repositório
            _mercadoPagoServiceMock.Verify(
                s => s.GetPaymentInfoAsync(It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Webhook_SemPaymentId_DeveRetornarBadRequest()
        {
            // ARRANGE
            var notification = new MercadoPagoNotification
            {
                Type = "payment",
                Data = new NotificationData { Id = "" } // ID vazio!
            };

            // ACT
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ============================================
        // TESTES DE REEMBOLSO
        // ============================================

        [Fact]
        public async Task Webhook_Reembolso_DeveDevolver_EstoqueAoStock()
        {
            // ARRANGE - Pedido já estava Pago
            var order = CreateFakeOrder(1, OrderStatus.Pago);
            order.OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, ProductName = "Produto Teste", UnitPrice = 99.90m, TotalPrice = 199.80m }
            };

            var product = CreateFakeProduct(1, stock: 8, reservedStock: 0); // Estoque já diminuído

            var paymentInfo = new MercadoPagoPaymentInfo
            {
                Status = "refunded",
                PaymentId = "123456",
                Amount = 199.80m,
                ExternalReference = "1"
            };

            _mercadoPagoServiceMock
                .Setup(s => s.GetPaymentInfoAsync("123456"))
                .ReturnsAsync(paymentInfo);

            _orderRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(order);

            _productRepositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _orderRepositoryMock
                .Setup(r => r.UpdateStatusAsync(1, OrderStatus.Cancelado))
                .ReturnsAsync(order);

            var notification = new MercadoPagoNotification
            {
                Type = "payment",
                Data = new NotificationData { Id = "123456" }
            };

            // ACT
            var result = await _controller.MercadoPagoWebhook(notification);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // Verificar que DEVOLVEU o estoque (8 + 2 = 10)
            _productRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Product>(p => p.Stock == 10)),
                Times.Once
            );
        }

        // ============================================
        // HELPERS - Criar objetos falsos
        // ============================================

        private Order CreateFakeOrder(long id, OrderStatus status)
        {
            return new Order
            {
                Id = id,
                NomeCompleto = "Teste",
                Email = "teste@teste.com",
                Telefone = "85999887766",
                Status = status,
                Total = 199.80m,
                OrderItems = new List<OrderItem>()
            };
        }

        private Product CreateFakeProduct(long id, int stock, int reservedStock)
        {
            return new Product
            {
                Id = id,
                Name = "Produto Teste",
                Price = 99.90m,
                Stock = stock,
                ReservedStock = reservedStock,
                IsActive = true,
                CategoryId = 1
            };
        }

        private StockReservation CreateFakeReservation(long id, long orderId, long productId, int quantity)
        {
            return new StockReservation
            {
                Id = id,
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                ExpireAt = DateTime.UtcNow.AddMinutes(30),
                IsReleased = false,
                Product = CreateFakeProduct(productId, stock: 10, reservedStock: quantity)
            };
        }
    }
}
