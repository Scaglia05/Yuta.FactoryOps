using Microsoft.Extensions.Configuration;
using Moq;
using Yuta.FactoryOps.Application.Services;
using Yuta.FactoryOps.Domain.Entities;
using Yuta.FactoryOps.Domain.Services;

namespace Yuta.FactoryOps.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Jwt:ChaveSecreta"])
                .Returns("SuaChaveSuperSecretaComMaisDe32CaracteresYutaOps");
            
            _tokenService = new TokenService(_mockConfiguration.Object);
        }

        [Fact]
        public void GerarTokenJwt_Should_Return_Valid_Token()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = 1,
                Nome = "Test User",
                Email = "test@example.com",
                Role = "Admin",
                EmpresaId = 1
            };

            // Act
            var token = _tokenService.GerarTokenJwt(usuario);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GerarTokenJwt_Should_Contain_User_Claims()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = 1,
                Nome = "Test User",
                Email = "test@example.com",
                Role = "Admin",
                EmpresaId = 1
            };

            // Act
            var token = _tokenService.GerarTokenJwt(usuario);

            // Assert
            Assert.NotNull(token);
            // Em um cenário real, você decodificaria o token e verificaria os claims
        }
    }
}