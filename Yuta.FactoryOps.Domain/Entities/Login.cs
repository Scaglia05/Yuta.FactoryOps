using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.Entities
{
    public class Login
    {
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail de operador é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um endereço de e-mail industrial válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha de acesso é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve conter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginAPI
    {
        public const string LoginEmailAPI = nameof(LoginEmailAPI);
        public const string LoginGooglelAPI = nameof(LoginGooglelAPI);
        public const string ConfirmarEmail = nameof(ConfirmarEmail);
        public const string GerarTokenConfirmacao = nameof(GerarTokenConfirmacao);
    }
}
