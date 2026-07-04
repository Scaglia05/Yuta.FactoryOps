using Microsoft.AspNetCore.Authorization;
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

            if (result.Sucesso && result.Usuario != null)
            {
                Empresa? empresa = await _empresaRepository.GetByIdAsync(result.Usuario.EmpresaId);

                var resultadoComEmpresa = new
                {
                    result.Sucesso,
                    result.Token,
                    result.Usuario,
                    Empresa = empresa != null ? new { empresa.Id, empresa.Nome, empresa.RazaoSocial, empresa.Cnpj } : null
                };

                return Ok(resultadoComEmpresa);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cadastro público (sem autenticação): cria a Empresa e o Usuário administrador
        /// que primeiro acessa o sistema. Usado pela tela "/cadastro" quando ainda não
        /// existe nenhum registro no banco de dados.
        /// </summary>
        [AllowAnonymous]
        [HttpPost(LoginAPI.CadastroAPI)]
        public async Task<IActionResult> Cadastrar([FromBody] CadastroRequestDto payload)
        {
            var result = await _authRepository.ExecutarCadastroAsync(payload);
            return Ok(result);
        }
    }
}