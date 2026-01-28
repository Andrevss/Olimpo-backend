namespace Olimpo.ProductAPI.Model.Entities
{
    public class Order : BaseEntity
    {
        //Informações do cliente
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }

        //Informações de entrega
        public string Rua { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Cep { get; set; }
        public string Complemento { get; set; }

        //Informações do pedido
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pendente;

        //Informações de pagamento
        public string? MercadoPagoId { get; set; }
        public string? MercadoPagoPaymentStatus { get; set; }

        //Relacionamento
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus {
        Pendente = 0,
        Pago = 1,
        Enviado = 2,
        Entregue = 3,
        Cancelado = 4,
        Rejeitado = 5
    }
}
