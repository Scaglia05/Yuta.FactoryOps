using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Services
{
    public interface IAuthService
    {
        Task<object> ValidarLoginEmailAsync(LoginRequestDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<object?> ValidarLoginExternoAsync(ExternoLoginDto dto);
    }
}