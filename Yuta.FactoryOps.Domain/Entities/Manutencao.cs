namespace Yuta.FactoryOps.Domain.Entities;

public class Manutencao
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty; 
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty; 
    public string DetalheAtivo { get; set; } = string.Empty; 
    public string Status { get; set; } = string.Empty; 
}