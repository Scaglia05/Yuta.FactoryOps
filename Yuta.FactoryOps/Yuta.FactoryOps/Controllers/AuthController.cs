using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Yuta.FactoryOps.Models;
using Yuta.FactoryOps.Models.DTO;
using Yuta.FactoryOps.Repositories.Interfaces;
using Yuta.FactoryOps.Application.DTOs; 

namespace Yuta.FactoryOps.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost(LoginAPI.LoginEmailAPI)]
        public async Task<IActionResult> LoginComEmail([FromBody] LoginRequest payload) // Corrigido para a nova arquitetura
        {
            var result = await _authRepository.ExecutarLoginEmailAsync(payload);
            return Ok(result);
        }

        [HttpPost(LoginAPI.LoginGooglelAPI)]
        public async Task<IActionResult> LoginComGoogle([FromBody] LoginRequest payload) // Corrigido para a nova arquitetura
        {
            var result = await _authRepository.ExecutarLoginGoogleAsync(payload);
            return Ok(result);
        }

        [HttpPost(UsuarioAPI.CriarUsuario)]
        public async Task<IActionResult> CriarUsuario([FromBody] RegistroUsuarioDto payload)
        {
            var result = await _authRepository.CriarUsuarioAsync(payload);
            return Ok(result);
        }

        [HttpGet(LoginAPI.ConfirmarEmail)]
        public async Task<IActionResult> ConfirmarEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result = await _authRepository.ConfirmarEmailAsync(email, token);
            return Ok(new { Sucesso = result });
        }

        [HttpPost(LoginAPI.GerarTokenConfirmacao)]
        public async Task<IActionResult> GerarTokenConfirmacao([FromBody] string email)
        {
            var result = await _authRepository.ExecutarGeracaoTokenEmailAsync(email);
            return Ok(result);
        }
    }
}