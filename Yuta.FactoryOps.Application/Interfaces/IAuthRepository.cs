using System.Threading.Tasks;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<object> ExecutarLoginEmailAsync(LoginRequestDto payload); 
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<object?> ExecutarLoginExternoAsync(ExternoLoginDto dto);
    }
}