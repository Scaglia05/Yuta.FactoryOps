using FluentValidation;
using Yuta.FactoryOps.Domain.DTOs;

namespace Yuta.FactoryOps.Application.Validators
{
    public class CadastroRequestValidator : AbstractValidator<CadastroRequestDto>
    {
        public CadastroRequestValidator()
        {
            RuleFor(x => x.NomeEmpresa)
                .NotEmpty().WithMessage("Nome da empresa é obrigatório")
                .MaximumLength(100);

            RuleFor(x => x.RazaoSocial)
                .NotEmpty().WithMessage("Razão social é obrigatória")
                .MaximumLength(200);

            RuleFor(x => x.Cnpj)
                .NotEmpty().WithMessage("CNPJ é obrigatório")
                .Length(14).WithMessage("CNPJ deve ter 14 dígitos (somente números)")
                .Matches("^[0-9]+$").WithMessage("CNPJ deve conter apenas números");

            RuleFor(x => x.NomeUsuario)
                .NotEmpty().WithMessage("Nome do responsável é obrigatório")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório")
                .EmailAddress().WithMessage("E-mail inválido");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

            RuleFor(x => x.ConfirmarSenha)
                .Equal(x => x.Senha).WithMessage("As senhas não coincidem");
        }
    }
}
