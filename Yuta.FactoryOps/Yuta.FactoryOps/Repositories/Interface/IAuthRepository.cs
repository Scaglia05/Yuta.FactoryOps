using System.Threading.Tasks;
using Yuta.FactoryOps.Models;
using Yuta.FactoryOps.Models.DTO;

namespace Yuta.FactoryOps.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        // Métodos de execução direta usados pelo Controller
        Task<object> ExecutarLoginEmailAsync(Login payload);
        Task<object> ExecutarLoginGoogleAsync(Login payload);
        Task<object> ExecutarGeracaoTokenEmailAsync(string email);

        // Métodos granulares da engrenagem
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload);
        Task<bool> ValidarSenhaAsync(Usuario usuario, string password);
        Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario);
        Task<bool> ConfirmarEmailAsync(string email, string token);
        Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl);
    }
}