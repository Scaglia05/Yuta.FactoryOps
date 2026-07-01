using Microsoft.AspNetCore.Mvc;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;

namespace Yuta.FactoryOps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresaController : ControllerBase
    {
        private readonly IEmpresaRepository _empresaRepository;

        public EmpresaController(IEmpresaRepository empresaRepository)
        {
            _empresaRepository = empresaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Empresa>>> GetEmpresas()
        {
            try
            {
                var empresas = await _empresaRepository.GetAllAsync();
                return Ok(empresas.ToList());
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar empresas" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> GetEmpresa(int id)
        {
            try
            {
                var empresa = await _empresaRepository.GetByIdAsync(id);
                if (empresa == null)
                {
                    return NotFound(new { mensagem = "Empresa não encontrada" });
                }
                return Ok(empresa);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar empresa" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Empresa>> CreateEmpresa([FromBody] Empresa empresa)
        {
            try
            {
                empresa.DataCadastro = DateTime.UtcNow;
                await _empresaRepository.AddAsync(empresa);
                return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.Id }, empresa);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao criar empresa" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Empresa>> UpdateEmpresa(int id, [FromBody] Empresa empresa)
        {
            try
            {
                if (id != empresa.Id)
                {
                    return BadRequest(new { mensagem = "ID mismatch" });
                }

                await _empresaRepository.UpdateAsync(empresa);
                return Ok(empresa);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao atualizar empresa" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmpresa(int id)
        {
            try
            {
                var empresa = await _empresaRepository.GetByIdAsync(id);
                if (empresa == null)
                {
                    return NotFound(new { mensagem = "Empresa não encontrada" });
                }

                await _empresaRepository.DeleteAsync(empresa);
                return Ok(new { mensagem = "Empresa excluída com sucesso" });
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao excluir empresa" });
            }
        }
    }
}