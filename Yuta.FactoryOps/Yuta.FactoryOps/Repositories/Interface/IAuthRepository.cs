using System.Threading.Tasks;
using Yuta.FactoryOps.Models;
using Yuta.FactoryOps.Models.DTO;
using Yuta.FactoryOps.Application.DTOs; 

namespace Yuta.FactoryOps.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<object> ExecutarLoginEmailAsync(LoginRequest payload); 
        Task<object> ExecutarLoginGoogleAsync(LoginRequest payload);
        Task<object> ExecutarGeracaoTokenEmailAsync(string email);
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario);
        Task<bool> ConfirmarEmailAsync(string email, string token);
        Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl);
    }
}