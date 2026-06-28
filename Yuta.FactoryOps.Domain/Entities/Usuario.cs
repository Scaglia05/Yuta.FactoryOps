using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.Entities
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string ProvedorAutenticacao { get; set; } = "Email";
        public bool EmailConfirmado { get; set; } = false;
        public string Role { get; set; } = "Operador";
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    public class UsuarioAPI
    {
        public const string CriarUsuario = nameof(CriarUsuario);
    }
}