using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Yuta.FactoryOps.Data;
using Yuta.FactoryOps.Models;
using Yuta.FactoryOps.Models.DTO;
using Yuta.FactoryOps.Repositories.Interfaces;

namespace Yuta.FactoryOps.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FactoryDbContext _context;

        public AuthRepository(FactoryDbContext context)
        {
            _context = context;
        }

        public async Task<object> ExecutarLoginEmailAsync(Login payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password))
            {
                return new { Sucesso = false, Mensagem = "E-mail e senha são obrigatórios." };
            }

            var usuario = await ObterPorEmailAsync(payload.Email);
            if (usuario == null)
            {
                return new { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            if (!usuario.EmailConfirmado)
            {
                return new { Sucesso = false, Status = 403, Mensagem = "Por favor, confirme seu e-mail antes de acessar a plataforma." };
            }

            var senhaValida = await ValidarSenhaAsync(usuario, payload.Password);
            if (!senhaValida)
            {
                return new { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            return new
            {
                Sucesso = true,
                Token = "JWT_TOKEN_TEMPORARIO_GERADO",
                Usuario = new { usuario.Nome, usuario.Email, usuario.Role, usuario.EmpresaId }
            };
        }

        public async Task<object> ExecutarLoginGoogleAsync(Login payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.IdToken))
            {
                return new { Sucesso = false, Mensagem = "Token do Google inválido." };
            }

            // TODO: Integrar Google.Apis.Auth para abrir o IdToken real
            string emailGoogle = "operador.exemplo@empresa.com";
            string nomeGoogle = "Operador Yuta";
            string fotoGoogle = "https://lh3.googleusercontent.com/...";

            var usuario = await ProcessarLoginGoogleAsync(emailGoogle, nomeGoogle, fotoGoogle);

            if (usuario == null)
            {
                return new { Sucesso = false, Mensagem = "Erro ao processar login com o Google. Verifique o provisionamento da empresa." };
            }

            return new
            {
                Sucesso = true,
                Token = "JWT_TOKEN_INTERNO_YUTA_VIA_GOOGLE",
                Usuario = new { usuario.Nome, usuario.Email, usuario.Role, usuario.EmpresaId }
            };
        }

        public async Task<object> ExecutarGeracaoTokenEmailAsync(string email)
        {
            var usuario = await ObterPorEmailAsync(email);
            if (usuario == null)
                return new { Sucesso = false, Mensagem = "Usuário não encontrado." };

            var token = await GerarTokenConfirmacaoEmailAsync(usuario);
            return new { Sucesso = true, Token = token };
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());
        }

        public async Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload)
        {
            var usuario = new Usuario
            {
                Email = payload.Email,
                Nome = payload.Nome,
                EmpresaId = payload.EmpresaId,
                Role = payload.Role
            };

            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (string.IsNullOrWhiteSpace(payload.Password)) throw new ArgumentException("A senha não pode ser vazia.", nameof(payload.Password));

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(payload.Password);
            usuario.ProvedorAutenticacao = "Email";
            usuario.DataCriacao = DateTime.UtcNow;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> ValidarSenhaAsync(Usuario usuario, string password)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash) || string.IsNullOrWhiteSpace(password))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }

        public async Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return await Task.FromResult(token);
        }

        public async Task<bool> ConfirmarEmailAsync(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token)) return false;

            var usuario = await ObterPorEmailAsync(email);
            if (usuario == null) return false;

            usuario.EmailConfirmado = true;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var usuario = await ObterPorEmailAsync(email);

            if (usuario != null)
            {
                if (usuario.ProvedorAutenticacao != "Google")
                {
                    usuario.ProvedorAutenticacao = "Google";
                    usuario.EmailConfirmado = true;
                    if (!string.IsNullOrEmpty(fotoUrl)) usuario.FotoUrl = fotoUrl;

                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                return usuario;
            }

            var empresaDefault = await _context.Empresas.FirstOrDefaultAsync();
            if (empresaDefault == null)
            {
                return null;
            }

            var novoUsuario = new Usuario
            {
                Email = email.ToLower().Trim(),
                Nome = nome,
                FotoUrl = fotoUrl,
                ProvedorAutenticacao = "Google",
                EmailConfirmado = true,
                EmpresaId = empresaDefault.Id,
                Role = "Operador",
                DataCriacao = DateTime.UtcNow
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();
            return novoUsuario;
        }
    }
}