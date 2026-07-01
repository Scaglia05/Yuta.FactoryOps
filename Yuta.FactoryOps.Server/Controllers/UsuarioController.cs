using Microsoft.AspNetCore.Mvc;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;

namespace Yuta.FactoryOps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllAsync();
                return Ok(usuarios.ToList());
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar usuários" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound(new { mensagem = "Usuário não encontrado" });
                }
                return Ok(usuario);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar usuário" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> CreateUsuario([FromBody] Usuario usuario)
        {
            try
            {
                usuario.DataCriacao = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                }
                await _usuarioRepository.AddAsync(usuario);
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao criar usuário" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Usuario>> UpdateUsuario(int id, [FromBody] Usuario usuario)
        {
            try
            {
                if (id != usuario.Id)
                {
                    return BadRequest(new { mensagem = "ID mismatch" });
                }

                var existingUser = await _usuarioRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { mensagem = "Usuário não encontrado" });
                }

                // Atualizar apenas campos permitidos
                existingUser.Nome = usuario.Nome;
                existingUser.Email = usuario.Email;
                existingUser.EmpresaId = usuario.EmpresaId;
                existingUser.Role = usuario.Role;
                existingUser.EmailConfirmado = usuario.EmailConfirmado;
                existingUser.FotoUrl = usuario.FotoUrl;
                existingUser.ProvedorAutenticacao = usuario.ProvedorAutenticacao;
                existingUser.ProviderKey = usuario.ProviderKey;

                // Atualizar senha apenas se fornecida
                if (!string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                }

                await _usuarioRepository.UpdateAsync(existingUser);
                return Ok(existingUser);
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao atualizar usuário" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound(new { mensagem = "Usuário não encontrado" });
                }

                await _usuarioRepository.DeleteAsync(usuario);
                return Ok(new { mensagem = "Usuário excluído com sucesso" });
            }
            catch
            {
                return StatusCode(500, new { mensagem = "Erro ao excluir usuário" });
            }
        }
    }
}