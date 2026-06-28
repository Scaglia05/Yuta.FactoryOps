using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Services
{
    public interface IAuthService
    {
        Task<object> ValidarLoginEmailAsync(LoginRequestDto payload);
        Task<object> ValidarLoginGoogleAsync(LoginRequestDto payload);
        Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario);
        Task<bool> ConfirmarEmailAsync(string email, string token);
        Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl);
    }
}