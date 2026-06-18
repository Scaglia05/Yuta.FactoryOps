using System.ComponentModel.DataAnnotations;

namespace Yuta.FactoryOps.Domain.Entities
{
    public class Login
    {
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "O identificador é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um formato de e-mail válido.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "A senha é obrigatória.")]
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
