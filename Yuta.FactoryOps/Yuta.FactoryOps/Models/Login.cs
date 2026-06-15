namespace Yuta.FactoryOps.Models
{
    public class Login
    {
        public string IdToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginAPI
    {
        public const string LoginEmailAPI = nameof(LoginEmailAPI);
        public const string LoginGooglelAPI = nameof(LoginGooglelAPI);
        public const string ConfirmarEmail = nameof(ConfirmarEmail);
        public const string GerarTokenConfirmacao = nameof(GerarTokenConfirmacao);
    }
}
