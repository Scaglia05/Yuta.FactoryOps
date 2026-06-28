using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.Entities
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativa { get; set; } = true;
    }
}