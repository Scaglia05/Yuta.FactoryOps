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

        public async Task<LoginResultDto> ValidarLoginEmailAsync(LoginRequestDto payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Senha))
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "E-mail e senha são obrigatórios." };
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(payload.Email);
            if (usuario == null)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            if (!usuario.EmailConfirmado)
            {
                return new LoginResultDto { Sucesso = false, Status = 403, Mensagem = "Por favor, confirme seu e-mail antes de acessar a plataforma." };
            }

            var senhaValida = await ValidarSenhaAsync(usuario, payload.Senha);
            if (!senhaValida)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Credenciais inválidas." };
            }

            string tokenReal = _tokenService.GerarTokenJwt(usuario);

            return new LoginResultDto
            {
                Sucesso = true,
                Token = tokenReal,
                Usuario = new UsuarioResumoDto { Nome = usuario.Nome, Email = usuario.Email, Role = usuario.Role, EmpresaId = usuario.EmpresaId }
            };
        }

        public async Task<LoginResultDto> ValidarCadastroAsync(CadastroRequestDto payload)
        {
            if (payload == null
                || string.IsNullOrWhiteSpace(payload.NomeEmpresa)
                || string.IsNullOrWhiteSpace(payload.RazaoSocial)
                || string.IsNullOrWhiteSpace(payload.Cnpj)
                || string.IsNullOrWhiteSpace(payload.NomeUsuario)
                || string.IsNullOrWhiteSpace(payload.Email)
                || string.IsNullOrWhiteSpace(payload.Senha))
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Preencha todos os campos obrigatórios." };
            }

            if (payload.Senha != payload.ConfirmarSenha)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "As senhas não coincidem." };
            }

            var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(payload.Email);
            if (usuarioExistente != null)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Este e-mail já está cadastrado." };
            }

            var empresaExistente = await _empresaRepository.ObterPorCnpjAsync(payload.Cnpj);
            if (empresaExistente != null)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Este CNPJ já está cadastrado. Solicite acesso a um administrador da empresa." };
            }

            var empresa = new Empresa
            {
                Nome = payload.NomeEmpresa,
                RazaoSocial = payload.RazaoSocial,
                Cnpj = payload.Cnpj,
                Ativa = true,
                DataCadastro = DateTime.UtcNow
            };
            await _empresaRepository.AddAsync(empresa);

            var usuario = new Usuario
            {
                EmpresaId = empresa.Id,
                Nome = payload.NomeUsuario,
                Email = payload.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(payload.Senha),
                Role = "Admin",
                EmailConfirmado = true,
                ProvedorAutenticacao = "Email",
                DataCriacao = DateTime.UtcNow
            };
            await _usuarioRepository.AddAsync(usuario);

            string token = _tokenService.GerarTokenJwt(usuario);

            return new LoginResultDto
            {
                Sucesso = true,
                Token = token,
                Usuario = new UsuarioResumoDto { Nome = usuario.Nome, Email = usuario.Email, Role = usuario.Role, EmpresaId = usuario.EmpresaId }
            };
        }

        public async Task<bool> ValidarSenhaAsync(Usuario usuario, string password)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash) || string.IsNullOrWhiteSpace(password))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }

        public async Task<LoginResultDto> ValidarLoginExternoAsync(ExternoLoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.ProviderKey))
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Dados de autenticação externa inválidos." };
            }

            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);

            if (usuario == null)
            {
                return new LoginResultDto { Sucesso = false, Mensagem = "Usuário não encontrado. Entre em contato com o administrador." };
            }

            // Verifica se o usuário já tem este provedor vinculado
            if (usuario.ProvedorAutenticacao != dto.Provider || usuario.ProviderKey != dto.ProviderKey)
            {
                // Atualiza para usar o provedor externo
                usuario.ProvedorAutenticacao = dto.Provider;
                usuario.ProviderKey = dto.ProviderKey;
                usuario.EmailConfirmado = true;
                if (!string.IsNullOrEmpty(dto.FotoUrl)) usuario.FotoUrl = dto.FotoUrl;

                await _usuarioRepository.UpdateAsync(usuario);
            }

            string tokenReal = _tokenService.GerarTokenJwt(usuario);

            return new LoginResultDto
            {
                Sucesso = true,
                Token = tokenReal,
                Usuario = new UsuarioResumoDto { Nome = usuario.Nome, Email = usuario.Email, Role = usuario.Role, EmpresaId = usuario.EmpresaId }
            };
        }
    }
}