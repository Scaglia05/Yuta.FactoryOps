using System.Threading.Tasks;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Server.Repositories.Interface
{
    public interface IAuthRepository
    {
        Task<object> ExecutarLoginEmailAsync(LoginRequestDto payload); 
        Task<object> ExecutarLoginGoogleAsync(LoginRequestDto payload);
        Task<object> ExecutarGeracaoTokenEmailAsync(string email);
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario);
        Task<bool> ConfirmarEmailAsync(string email, string token);
        Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl);
    }
}