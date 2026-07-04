using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.DTOs;

/// <summary>
/// Dados enviados pela tela pública de Cadastro para criar, em uma única
/// operação, a Empresa e o Usuário administrador que primeiro acessa o sistema.
/// </summary>
public class CadastroRequestDto
{
    [Required(ErrorMessage = "O nome da empresa é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome da empresa deve ter no máximo 100 caracteres")]
    public string NomeEmpresa { get; set; } = string.Empty;

    [Required(ErrorMessage = "A razão social é obrigatória")]
    [StringLength(200, ErrorMessage = "A razão social deve ter no máximo 200 caracteres")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter 14 dígitos (somente números)")]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome do responsável é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string NomeUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha")]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}
