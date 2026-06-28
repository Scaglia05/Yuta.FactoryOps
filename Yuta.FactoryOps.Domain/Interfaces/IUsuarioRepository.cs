using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Interfaces
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> ObterPorEmailAsync(string email);
    }
}