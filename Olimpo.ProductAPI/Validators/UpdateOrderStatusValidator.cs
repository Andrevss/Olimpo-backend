using FluentValidation;
using Olimpo.ProductAPI.Model.DTOs;

namespace Olimpo.ProductAPI.Validators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDTO>
    {
        public UpdateOrderStatusValidator() 
        {
            RuleFor(o => o.Status)
                .NotEmpty().WithMessage("Status é obrigatório.")
                .Must(BeAValidStatus).WithMessage("Status inválido. Use: Pendente, Pago, Enviado, Entregue, Cancelado ou Rejeitado.");
        }

        private bool BeAValidStatus(string status)
        {
            var validStatuses = new[] { "Pendente", "Pago", "Enviado", "Entregue", "Cancelado", "Rejeitado" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

    }
}
