using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string? ProviderKey { get; set; }
        public bool EmailConfirmado { get; set; } = false;
        public string Role { get; set; } = "Operador";
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        // Propriedade auxiliar para formulários (não mapeada no banco)
        [NotMapped]
        public string? Password { get; set; }
    }

    public class UsuarioAPI
    {
        public const string CriarUsuario = nameof(CriarUsuario);
    }
}