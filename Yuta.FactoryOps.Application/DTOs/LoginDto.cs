using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "O identificador é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um formato de e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}