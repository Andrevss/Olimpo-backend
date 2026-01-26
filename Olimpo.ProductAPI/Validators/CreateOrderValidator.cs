using Olimpo.ProductAPI.Model.DTOs;
using FluentValidation;

namespace Olimpo.ProductAPI.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderDTO>
    {
        public CreateOrderValidator() 
        {
            RuleFor(order => order.NomeCompleto)
                .NotEmpty().WithMessage("O nome completo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome completo deve ter no máximo 100 caracteres.");
            
            RuleFor(order => order.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("O email deve ser válido.");
           
            RuleFor(order => order.Telefone)
                .NotEmpty().WithMessage("O telefone é obrigatório.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("O telefone deve ser válido.");

            RuleFor(o => o.Cep)
                .NotEmpty().WithMessage("CEP é obrigatório")
                .Matches(@"^\d{8}$").WithMessage("CEP deve ter 8 dígitos (apenas números)")
                .MaximumLength(9).WithMessage("CEP muito longo");

            RuleFor(order => order.Rua)
                .NotEmpty().WithMessage("A rua é obrigatória.")
                .MaximumLength(150).WithMessage("A rua deve ter no máximo 150 caracteres.");
            
            RuleFor(order => order.Numero)
                .NotEmpty().WithMessage("O número é obrigatório.")
                .MaximumLength(10).WithMessage("O número deve ter no máximo 10 caracteres.");

            RuleFor(order => order.Estado)
                .NotEmpty().WithMessage("O estado é obrigatório.")
                .MaximumLength(30).WithMessage("A rua deve ter no máximo 30 caracteres.");

            RuleFor(order => order.Bairro)
                .NotEmpty().WithMessage("O bairro é obrigatório.")
                .MaximumLength(100).WithMessage("O bairro deve ter no máximo 100 caracteres.");
            
            RuleFor(order => order.Cidade)
                .NotEmpty().WithMessage("A cidade é obrigatória.")
                .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres.");
           
            RuleFor(order => order.Items)
                .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("O pedido deve conter pelo menos um item.");
            
            RuleForEach(order => order.Items).SetValidator(new CreateOrderItemValidator());
        }
    }
    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDTO>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(item => item.ProductId)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");
            
            RuleFor(item => item.Quantity)
                .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        }
    }
}
