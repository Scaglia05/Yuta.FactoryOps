using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;
using Yuta.FactoryOps.Domain.Services;

namespace Yuta.FactoryOps.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IEmpresaRepository empresaRepository,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _empresaRepository = empresaRepository;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<object> ValidarLoginEmailAsync(LoginRequestDto payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Senha))
            {
                return new { Sucesso = false, Mensagem = "E-mail e senha são obrigatórios." };
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(payload.Email);
            if (usuario == null)
            {
                return new { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            if (!usuario.EmailConfirmado)
            {
                return new { Sucesso = false, Status = 403, Mensagem = "Por favor, confirme seu e-mail antes de acessar a plataforma." };
            }

            var senhaValida = await ValidarSenhaAsync(usuario, payload.Senha);
            if (!senhaValida)
            {
                return new { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            string tokenReal = _tokenService.GerarTokenJwt(usuario);

            return new
            {
                Sucesso = true,
                Token = tokenReal,
                Usuario = new { usuario.Nome, usuario.Email, usuario.Role, usuario.EmpresaId }
            };
        }

        public async Task<object> ValidarLoginGoogleAsync(LoginRequestDto payload)
        {
            if (payload == null)
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

            string tokenReal = _tokenService.GerarTokenJwt(usuario);

            return new
            {
                Sucesso = true,
                Token = tokenReal,
                Usuario = new { usuario.Nome, usuario.Email, usuario.Role, usuario.EmpresaId }
            };
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

            if (string.IsNullOrWhiteSpace(payload.Password)) 
                throw new ArgumentException("A senha não pode ser vazia.", nameof(payload.Password));

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(payload.Password);
            usuario.ProvedorAutenticacao = "Email";
            usuario.DataCriacao = DateTime.UtcNow;

            await _usuarioRepository.AddAsync(usuario);
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

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null) return false;

            usuario.EmailConfirmado = true;

            await _usuarioRepository.UpdateAsync(usuario);
            return true;
        }

        public async Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario != null)
            {
                if (usuario.ProvedorAutenticacao != "Google")
                {
                    usuario.ProvedorAutenticacao = "Google";
                    usuario.EmailConfirmado = true;
                    if (!string.IsNullOrEmpty(fotoUrl)) usuario.FotoUrl = fotoUrl;

                    await _usuarioRepository.UpdateAsync(usuario);
                }
                return usuario;
            }

            var empresaDefault = (await _empresaRepository.GetAllAsync()).FirstOrDefault();
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

            await _usuarioRepository.AddAsync(novoUsuario);
            return novoUsuario;
        }
    }
}