using System.Threading.Tasks;
using Yuta.FactoryOps.Models;

namespace Yuta.FactoryOps.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<Usuario> CriarUsuarioAsync(Usuario usuario, string password);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario);
        Task<bool> ConfirmarEmailAsync(string email, string token);
        Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl);
    }
}