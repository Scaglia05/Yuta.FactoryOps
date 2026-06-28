using FluentValidation;
using Yuta.FactoryOps.Domain.DTOs;

namespace Yuta.FactoryOps.Application.Validators
{
    public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
    {
        public RegistroUsuarioValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório")
                .EmailAddress().WithMessage("E-mail inválido");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Senha deve conter letras maiúsculas, minúsculas e números");

            RuleFor(x => x.EmpresaId)
                .GreaterThan(0).WithMessage("EmpresaId deve ser maior que 0");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role é obrigatória")
                .Must(role => new[] { "Admin", "Gerente", "Operador" }.Contains(role))
                .WithMessage("Role deve ser Admin, Gerente ou Operador");
        }
    }
}