using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Domain.Services
{
    public interface ITokenService
    {
        string GerarTokenJwt(Usuario usuario);
    }
}