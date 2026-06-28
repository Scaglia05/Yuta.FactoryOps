using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Interfaces
{
    public interface IEmpresaRepository : IRepository<Empresa>
    {
        Task<Empresa?> ObterPorCnpjAsync(string cnpj);
    }
}