using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Olimpo.ProductAPI.Model.DTOs;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;
using Olimpo.ProductAPI.Services;

namespace Olimpo.ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockReservationRepository _stockReservationRepository;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderRepository orderRepository, IProductRepository productRepository, IStockReservationRepository stockReservationRepository ,IMercadoPagoService mercadoPagoService, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stockReservationRepository = stockReservationRepository;
            _mercadoPagoService = mercadoPagoService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAll()
        {
            var orders = await _orderRepository.GetAllAsync();
            var ordersDto = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return Ok(ordersDto);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> GetById(long id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Pedido não encontrado" });
            }
            var orderDto = _mapper.Map<OrderDTO>(order);
            return Ok(orderDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDTO>> Create([FromBody] CreateOrderDTO orderDto)
        {
            if (orderDto.Items == null || !orderDto.Items.Any())
            {
                return BadRequest(new { message = "O pedido deve conter pelo menos 1 item" });
            }

            var orderItems = new List<OrderItem>();
            var productsToUpdate = new List<Product>();
            var variantsToUpdate = new List<ProductVariant>();
            var reservations = new List<StockReservation>();
            decimal totalAmount = 0;

            foreach (var item in orderDto.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest(new { message = "Quantidade do item deve ser maior que zero" });
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product == null)
                    return BadRequest(new { message = $"Produto ID {item.ProductId} não encontrado" });

                if (!product.IsActive)
                    return BadRequest(new { message = $"Produto '{product.Name}' não está disponível" });

                ProductVariant? variant = null;
                if (item.ProductVariantId.HasValue)
                {
                    variant = await _productRepository.GetVariantByIdAsync(item.ProductVariantId.Value);
                    if (variant == null || variant.ProductId != product.Id)
                    {
                        return BadRequest(new { message = $"Variação ID {item.ProductVariantId.Value} não encontrada para o produto '{product.Name}'" });
                    }
                }
                else if (!string.IsNullOrWhiteSpace(item.Size))
                {
                    variant = await _productRepository.GetVariantByProductAndSizeAsync(product.Id, item.Size.Trim().ToUpperInvariant());
                    if (variant == null)
                    {
                        return BadRequest(new { message = $"Tamanho '{item.Size}' não encontrado para o produto '{product.Name}'" });
                    }
                }

                var availableStock = variant?.AvailableStock ?? product.AvailableStock;
                if (availableStock < item.Quantity)
                {
                    return BadRequest(new
                    {
                        message = variant == null
                            ? $"Estoque insuficiente para '{product.Name}'. Disponível: {availableStock}"
                            : $"Estoque insuficiente para '{product.Name}' tamanho '{variant.Size}'. Disponível: {availableStock}"
                    });
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductVariantId = variant?.Id,
                    ProductName = product.Name,
                    Size = variant?.Size,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = product.Price * item.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;

                // ✅ RESERVAR ESTOQUE (não diminui Stock, só ReservedStock)
                if (variant != null)
                {
                    variant.ReservedStock += item.Quantity;
                    variant.UpdatedAt = DateTime.UtcNow;
                    variantsToUpdate.Add(variant);
                }
                else
                {
                    product.ReservedStock += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    productsToUpdate.Add(product);
                }
            }

            // Criar pedido
            var order = _mapper.Map<Order>(orderDto);
            order.OrderItems = orderItems;
            order.Total = totalAmount;
            order.Status = OrderStatus.Pendente;

            var createdOrder = await _orderRepository.CreateAsync(order);

            // ✅ SALVAR RESERVAS DE ESTOQUE (expira em 30 minutos)
            foreach (var item in orderItems)
            {
                var reservation = new StockReservation
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    OrderId = createdOrder.Id,
                    Quantity = item.Quantity,
                    ExpireAt = DateTime.UtcNow.AddMinutes(30) // ⏱️ 30 minutos para pagar
                };

                reservations.Add(reservation);
                await _stockReservationRepository.CreateAsync(reservation);
            }

            // Atualizar ReservedStock dos produtos
            foreach (var product in productsToUpdate)
            {
                await _productRepository.UpdateAsync(product);
            }

            foreach (var variant in variantsToUpdate)
            {
                await _productRepository.UpdateVariantAsync(variant);
            }

            // Criar preferência no Mercado Pago
            string paymentUrl;
            try
            {
                paymentUrl = await _mercadoPagoService.CreatePreferenceAsync(createdOrder);
                createdOrder.MercadoPagoId = paymentUrl.Split('/').Last();
                await _orderRepository.UpdateStatusAsync(createdOrder.Id, OrderStatus.Pendente);
            }
            catch (Exception ex)
            {
                // Se falhar, liberar reservas
                foreach (var reservation in reservations)
                {
                    await _stockReservationRepository.ReleaseReservationAsync(reservation.Id);
                }
                return StatusCode(500, new { message = "Erro ao gerar link de pagamento", error = ex.Message });
            }

            var resultDto = _mapper.Map<OrderDTO>(createdOrder);
            resultDto.PaymentUrl = paymentUrl;

            return CreatedAtAction(
                nameof(GetById),
                new { id = resultDto.Id },
                resultDto
            );
        }

        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDTO>> UpdateStatus(
            long id,
            [FromBody] UpdateOrderStatusDTO statusDto)
        {
            if (!await _orderRepository.ExistsAsync(id))
                return NotFound(new { message = "Pedido não encontrado" });

            // Converter string para enum
            if (!Enum.TryParse<OrderStatus>(statusDto.Status, true, out var status))
                return BadRequest(new { message = "Status inválido" });

            var updatedOrder = await _orderRepository.UpdateStatusAsync(id, status);
            var resultDto = _mapper.Map<OrderDTO>(updatedOrder);

            return Ok(resultDto);
        }
    }
}
