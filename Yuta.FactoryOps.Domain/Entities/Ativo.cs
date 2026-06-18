namespace Yuta.FactoryOps.Domain.Entities;

public class Ativo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int SaudePercentual { get; set; } // Ex: 89%, 72%
    public string ImagemUrl { get; set; } = string.Empty;
    public List<int> HistoricoSaude { get; set; } = new();
    public bool StatusOnline { get; set; } = true;
}