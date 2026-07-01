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

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return await _usuarioRepository.ObterPorEmailAsync(email);
        }

        public async Task<bool> ValidarSenhaAsync(Usuario usuario, string password)
        {
            return await _authService.ValidarSenhaAsync(usuario, password);
        }

        public async Task<object?> ExecutarLoginExternoAsync(ExternoLoginDto dto)
        {
            return await _authService.ValidarLoginExternoAsync(dto);
        }
    }
}