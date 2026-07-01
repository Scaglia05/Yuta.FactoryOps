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

        public async Task<bool> ValidarSenhaAsync(Usuario usuario, string password)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash) || string.IsNullOrWhiteSpace(password))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }

        public async Task<object?> ValidarLoginExternoAsync(ExternoLoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.ProviderKey))
            {
                return new { Sucesso = false, Mensagem = "Dados de autenticação externa inválidos." };
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);

            if (usuario == null)
            {
                return new { Sucesso = false, Mensagem = "Usuário não encontrado. Entre em contato com o administrador." };
            }

            // Verifica se o usuário já tem este provedor vinculado
            if (usuario.ProvedorAutenticacao != dto.Provider)
            {
                // Atualiza para usar o provedor externo
                usuario.ProvedorAutenticacao = dto.Provider;
                usuario.EmailConfirmado = true;
                if (!string.IsNullOrEmpty(dto.FotoUrl)) usuario.FotoUrl = dto.FotoUrl;
                
                await _usuarioRepository.UpdateAsync(usuario);
            }

            string tokenReal = _tokenService.GerarTokenJwt(usuario);

            return new
            {
                Sucesso = true,
                Token = tokenReal,
                Usuario = new { usuario.Nome, usuario.Email, usuario.Role, usuario.EmpresaId }
            };
        }
    }
}