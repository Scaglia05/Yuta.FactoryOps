using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuta.FactoryOps.Data;
using Yuta.FactoryOps.Models;
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

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());
        }

        public async Task<Usuario> CriarUsuarioAsync(Usuario usuario, string password)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("A senha não pode ser vazia.", nameof(password));

            // Gera o Hash da senha usando Salt interno do BCrypt (Segurança nível bancário)
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
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

            // O BCrypt descriptografa internamente o hash e valida se a senha bate
            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }

        public async Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            // Cria um identificador único universal codificado em Base64 para encurtar o link do e-mail
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // TODO: Salvar esse token em uma tabela dedicada 'TokensSeguranca' com tempo de expiração
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