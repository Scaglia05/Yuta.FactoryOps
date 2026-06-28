using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Yuta.FactoryOps.Application.DTOs;
using Yuta.FactoryOps.Domain.DTOs;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;
using Yuta.FactoryOps.Domain.Services;

namespace Yuta.FactoryOps.Infrastructure.Repositories
{
    public class AuthRepository : Yuta.FactoryOps.Application.Interfaces.IAuthRepository
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;

        public AuthRepository(IAuthService authService, IUsuarioRepository usuarioRepository)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<object> ExecutarLoginEmailAsync(LoginRequestDto payload)
        {
            return await _authService.ValidarLoginEmailAsync(payload);
        }

        public async Task<object> ExecutarLoginGoogleAsync(LoginRequestDto payload)
        {
            return await _authService.ValidarLoginGoogleAsync(payload);
        }

        public async Task<object> ExecutarGeracaoTokenEmailAsync(string email)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return new { Sucesso = false, Mensagem = "Usuário não encontrado." };

            var token = await _authService.GerarTokenConfirmacaoEmailAsync(usuario);
            return new { Sucesso = true, Token = token };
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _usuarioRepository.ObterPorEmailAsync(email);
        }

        public async Task<Usuario> CriarUsuarioAsync(RegistroUsuarioDto payload)
        {
            return await _authService.CriarUsuarioAsync(payload);
        }

        public async Task<bool> ValidarSenhaAsync(Usuario usuario, string password)
        {
            return await _authService.ValidarSenhaAsync(usuario, password);
        }

        public async Task<string> GerarTokenConfirmacaoEmailAsync(Usuario usuario)
        {
            return await _authService.GerarTokenConfirmacaoEmailAsync(usuario);
        }

        public async Task<bool> ConfirmarEmailAsync(string email, string token)
        {
            return await _authService.ConfirmarEmailAsync(email, token);
        }

        public async Task<Usuario?> ProcessarLoginGoogleAsync(string email, string nome, string fotoUrl)
        {
            return await _authService.ProcessarLoginGoogleAsync(email, nome, fotoUrl);
        }
    }
}