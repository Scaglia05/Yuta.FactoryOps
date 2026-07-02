using Microsoft.AspNetCore.Mvc;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Application.Interfaces;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Interfaces;

namespace Yuta.FactoryOps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmpresaRepository _empresaRepository;

        public AuthController(IAuthRepository authRepository, IEmpresaRepository empresaRepository)
        {
            _authRepository = authRepository;
            _empresaRepository = empresaRepository;
        }

        [HttpPost(LoginAPI.LoginEmailAPI)]
        public async Task<IActionResult> LoginComEmail([FromBody] LoginRequestDto payload)
        {
            var result = await _authRepository.ExecutarLoginEmailAsync(payload);
            
            if (result != null && result.GetType().GetProperty("Sucesso")?.GetValue(result) is bool sucesso && sucesso)
            {
                // Extrair EmpresaId do usuário no resultado
                var usuario = result.GetType().GetProperty("Usuario")?.GetValue(result);
                var empresaIdProp = usuario?.GetType().GetProperty("EmpresaId");
                var empresaIdObj = empresaIdProp?.GetValue(usuario);
                int empresaId = 0;
                
                if (empresaIdObj != null)
                {
                    empresaId = Convert.ToInt32(empresaIdObj);
                }
                
                // Buscar informações da empresa
                Empresa? empresa = null;
                if (empresaId > 0)
                {
                    empresa = await _empresaRepository.GetByIdAsync(empresaId);
                }
                
                // Adicionar empresa ao resultado
                var resultadoComEmpresa = new
                {
                    Sucesso = true,
                    Token = result.GetType().GetProperty("Token")?.GetValue(result),
                    Usuario = usuario,
                    Empresa = empresa != null ? new { empresa.Id, empresa.Nome, empresa.RazaoSocial, empresa.Cnpj } : null
                };
                
                return Ok(resultadoComEmpresa);
            }
            
            return Ok(result);
        }
    }
}