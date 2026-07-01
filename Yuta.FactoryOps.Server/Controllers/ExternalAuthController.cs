using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Services;

namespace Yuta.FactoryOps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public ExternalAuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpGet("google")]
        public async Task<IActionResult> GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Redirect("/login?error=google_failed");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var providerKey = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            {
                return Redirect("/login?error=invalid_google_data");
            }

            // Tenta fazer login com o provedor externo
            var externalLoginDto = new ExternoLoginDto
            {
                Provider = "Google",
                ProviderKey = providerKey,
                Email = email,
                Nome = name ?? email.Split('@')[0]
            };

            var loginResult = await _authRepository.ExecutarLoginExternoAsync(externalLoginDto);
            var token = loginResult?.GetType().GetProperty("Token")?.GetValue(loginResult, null) as string;

            if (loginResult != null && !string.IsNullOrEmpty(token))
            {
                // Redireciona para o frontend com o token
                return Redirect($"/login?token={token}&provider=google");
            }

            // Se o usuário não existe, retorna erro
            return Redirect("/login?error=user_not_found");
        }

        [HttpGet("microsoft")]
        public async Task<IActionResult> MicrosoftLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftCallback))
            };
            
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        [HttpGet("microsoft-callback")]
        public async Task<IActionResult> MicrosoftCallback()
        {
            var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Redirect("/login?error=microsoft_failed");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var providerKey = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            {
                return Redirect("/login?error=invalid_microsoft_data");
            }

            // Tenta fazer login com o provedor externo
            var externalLoginDto = new ExternoLoginDto
            {
                Provider = "Microsoft",
                ProviderKey = providerKey,
                Email = email,
                Nome = name ?? email.Split('@')[0]
            };

            var loginResult = await _authRepository.ExecutarLoginExternoAsync(externalLoginDto);
            var token = loginResult?.GetType().GetProperty("Token")?.GetValue(loginResult, null) as string;

            if (loginResult != null && !string.IsNullOrEmpty(token))
            {
                // Redireciona para o frontend com o token
                return Redirect($"/login?token={token}&provider=microsoft");
            }

            // Se o usuário não existe, retorna erro
            return Redirect("/login?error=user_not_found");
        }
    }
}