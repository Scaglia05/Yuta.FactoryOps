using Microsoft.AspNetCore.Mvc;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Domain.DTOs;

namespace Yuta.FactoryOps.Server.Controllers
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
        public async Task<IActionResult> LoginComEmail([FromBody] LoginRequestDto payload)
        {
            var result = await _authRepository.ExecutarLoginEmailAsync(payload);
            return Ok(result);
        }
    }
}