namespace Yuta.FactoryOps.Models.DTO
{
    public class RegistroUsuarioDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public Guid EmpresaId { get; set; }
        public string Role { get; set; } = "Operador";
    }
}
