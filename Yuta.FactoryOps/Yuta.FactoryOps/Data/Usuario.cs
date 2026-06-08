using System;

namespace Yuta.FactoryOps.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string ProvedorAutenticacao { get; set; } = "Email";
        public bool EmailConfirmado { get; set; } = false;
        public Guid EmpresaId { get; set; }
        public string Role { get; set; } = "Operador";
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}