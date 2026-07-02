using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Yuta.FactoryOps.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            IEmpresaRepository empresaRepository,
            IUsuarioRepository usuarioRepository,
            ILogger<DatabaseSeeder> logger)
        {
            _empresaRepository = empresaRepository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedEmpresaPadraoAsync();
                await SeedUsuarioPadraoAsync();
            }
            catch (Npgsql.PostgresException ex)
            {
                _logger.LogWarning(ex, "Banco de dados não disponível ou credenciais inválidas. Seed não executado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao popular banco de dados");
            }
        }

        private async Task SeedEmpresaPadraoAsync()
        {
            var empresas = await _empresaRepository.GetAllAsync();
            
            if (!empresas.Any())
            {
                var empresaPadrao = new Empresa
                {
                    RazaoSocial = "Yuta Factory Operations",
                    Nome = "Yuta",
                    Cnpj = "00000000000100",
                    Ativa = true,
                    DataCadastro = DateTime.UtcNow
                };

                await _empresaRepository.AddAsync(empresaPadrao);
                _logger.LogInformation("Empresa padrão criada: {Nome}", empresaPadrao.Nome);
            }
        }

        private async Task SeedUsuarioPadraoAsync()
        {
            var empresas = await _empresaRepository.GetAllAsync();
            var empresaPadrao = empresas.FirstOrDefault();

            if (empresaPadrao == null)
            {
                _logger.LogWarning("Empresa padrão não encontrada. Não é possível criar usuário padrão.");
                return;
            }

            var usuarios = await _usuarioRepository.GetAllAsync();
            var usuarioExistente = usuarios.FirstOrDefault(u => u.Email == "admin@yuta.com");

            if (usuarioExistente == null)
            {
                var usuarioPadrao = new Usuario
                {
                    Email = "admin@yuta.com",
                    Nome = "Administrador Yuta",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    EmpresaId = empresaPadrao.Id,
                    EmailConfirmado = true,
                    ProvedorAutenticacao = "Email",
                    DataCriacao = DateTime.UtcNow
                };

                await _usuarioRepository.AddAsync(usuarioPadrao);
                _logger.LogInformation("Usuário padrão criado: {Email}", usuarioPadrao.Email);
            }
        }
    }
}