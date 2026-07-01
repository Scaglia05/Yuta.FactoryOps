namespace Yuta.FactoryOps.Domain.DTOs;

public class ExternoLoginDto
{
    public string Provider { get; set; } = string.Empty; // "Google" ou "Microsoft"
    public string ProviderKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
}