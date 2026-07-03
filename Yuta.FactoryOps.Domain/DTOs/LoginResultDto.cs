namespace Yuta.FactoryOps.Domain.DTOs;

/// <summary>
/// Resultado padronizado de uma tentativa de login (email/senha ou externo).
/// Substitui os antigos retornos "object" (anonymous types), que exigiam
/// reflection no controller e impediam checagens fortemente tipadas no client.
/// </summary>
public class LoginResultDto
{
    public bool Sucesso { get; set; }
    public string? Token { get; set; }
    public string? Mensagem { get; set; }
    public int? Status { get; set; }
    public UsuarioResumoDto? Usuario { get; set; }
}

public class UsuarioResumoDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
}
