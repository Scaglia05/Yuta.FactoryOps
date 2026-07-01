using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.Entities
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "A razão social é obrigatória")]
        [StringLength(200, ErrorMessage = "A razão social deve ter no máximo 200 caracteres")]
        public string RazaoSocial { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O CNPJ é obrigatório")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter 14 caracteres")]
        public string Cnpj { get; set; } = string.Empty;
        
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativa { get; set; } = true;
    }
}