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
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderRepository orderRepository, IProductRepository productRepository, IMercadoPagoService mercadoPagoService, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
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
            // Validar e buscar produtos (código existente mantém)
            var orderItems = new List<OrderItem>();
            var productsToUpdate = new List<Product>();
            decimal totalAmount = 0;

            foreach (var item in orderDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product == null)
                    return BadRequest(new { message = $"Produto ID {item.ProductId} não encontrado" });

                if (!product.IsActive)
                    return BadRequest(new { message = $"Produto '{product.Name}' não está disponível" });

                if (product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Estoque insuficiente para '{product.Name}'. Disponível: {product.Stock}" });

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    TotalPrice = product.Price * item.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;

                product.Stock -= item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                productsToUpdate.Add(product);
            }

            // Criar pedido
            var order = _mapper.Map<Order>(orderDto);
            order.OrderItems = orderItems;
            order.Total = totalAmount;
            order.Status = OrderStatus.Pendente;

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Salvar alterações no estoque
            foreach (var product in productsToUpdate)
            {
                await _productRepository.UpdateAsync(product);
            }

            // CRIAR PREFERÊNCIA NO MERCADO PAGO
            string paymentUrl;
            try
            {
                paymentUrl = await _mercadoPagoService.CreatePreferenceAsync(createdOrder);

                // Salvar ID da preferência no pedido
                createdOrder.MercadoPagoId = paymentUrl.Split('/').Last(); // Extrai ID da URL
                await _orderRepository.UpdateStatusAsync(createdOrder.Id, OrderStatus.Pendente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar link de pagamento", error = ex.Message });
            }

            var resultDto = _mapper.Map<OrderDTO>(createdOrder);
            resultDto.PaymentUrl = paymentUrl; // Retorna URL para o frontend

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
