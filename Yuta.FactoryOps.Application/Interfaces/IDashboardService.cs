using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Application.Interfaces;

public interface IDashboardService
{
    Task<List<Ativo>> ObterAtivosAsync();
    Task<List<Manutencao>> ObterManutencoesAsync();
}