using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Application.Interfaces.Services;

public class DashboardMockService : IDashboardService
{
    public async Task<List<Ativo>> ObterAtivosAsync()
    {
        await Task.Delay(300); // Simula latência de rede da API
        return new List<Ativo>
        {
            new() { Id = 1, Nome = "Corte Laser CL-01", Descricao = "Máquina de Corte a Laser 4kW", SaudePercentual = 89, StatusOnline = true, HistoricoSaude = new(){8,9,7,10,9,8,9} },
            new() { Id = 2, Nome = "Prensa Hidráulica PH-02", Descricao = "Prensa Hidráulica 200 ton", SaudePercentual = 72, StatusOnline = true, HistoricoSaude = new(){5,6,4,7,6,5,4} },
            new() { Id = 3, Nome = "Caldeira Industrial CA-03", Descricao = "Caldeira a Vapor 15 t/h", SaudePercentual = 92, StatusOnline = true, HistoricoSaude = new(){9,9,10,9,10,9,9} },
            new() { Id = 4, Nome = "Rolo Compressor RC-04", Descricao = "Laminador de Conformação", SaudePercentual = 85, StatusOnline = true, HistoricoSaude = new(){8,8,7,9,8,8,8} }
        };
    }

    public async Task<List<Manutencao>> ObterManutencoesAsync()
    {
        await Task.Delay(300);
        return new List<Manutencao>
        {
            new() { Id = 1, Data = "09/04", Hora = "06:00", Tipo = "Preventiva", Titulo = "Troca de óleo hidráulico ISO VG 46 e filtros", DetalheAtivo = "Prensa Hidráulica PH-02 — Carlos M.", Status = "Concluido" },
            new() { Id = 2, Data = "08/04", Hora = "14:30", Tipo = "Preditiva", Titulo = "Substituição de lente focalizadora — degradação detectada via FFT", DetalheAtivo = "Corte Laser CL-01 — Ana R.", Status = "Concluido" },
            new() { Id = 3, Data = "08/04", Hora = "08:00", Tipo = "Preventiva", Titulo = "Inspeção de tubulações e válvulas de segurança (NR-13)", DetalheAtivo = "Caldeira Industrial CA-03 — João P.", Status = "Concluido" },
            new() { Id = 4, Data = "07/04", Hora = "22:15", Tipo = "Corretiva", Titulo = "Recalibração de encoder do eixo 3 — deriva detectada", DetalheAtivo = "Manipulador Robótico MR-06 — Pedro S.", Status = "Concluido" }
        };
    }
}