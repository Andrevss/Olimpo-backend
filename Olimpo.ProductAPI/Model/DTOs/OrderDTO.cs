namespace Olimpo.ProductAPI.Model.DTOs
{
    public class OrderDTO
    {
        public long Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? PaymentUrl { get; set; } 
        public List<OrderItemDTO> Items { get; set; } = new();
    }

    public class OrderItemDTO
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? ProductVariantId { get; set; }
        public string? Size { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateOrderDTO
    {
        // Dados do Cliente
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;

        // Endereço
        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;

        // Itens do Pedido
        public List<CreateOrderItemDTO> Items { get; set; } = new();
    }

    public class CreateOrderItemDTO
    {
        public long ProductId { get; set; }
        public long? ProductVariantId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        public string Status { get; set; } = string.Empty; // "Pendente", "Pago", "Enviado", etc
    }
}
