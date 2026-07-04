using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Services
{
    public interface IAuthService
    {
        Task<LoginResultDto> ValidarLoginEmailAsync(LoginRequestDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<LoginResultDto> ValidarLoginExternoAsync(ExternoLoginDto dto);
        Task<LoginResultDto> ValidarCadastroAsync(CadastroRequestDto payload);
    }
}