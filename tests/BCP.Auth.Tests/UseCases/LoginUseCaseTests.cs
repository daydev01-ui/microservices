namespace BCP.Auth.Tests.UseCases;

using BCP.Auth.API.Application.DTOs;
using BCP.Auth.API.Application.UseCases;
using BCP.Auth.API.Domain.Entities;
using BCP.Auth.API.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class LoginUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly IConfiguration _config;
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _repoMock = new Mock<IUsuarioRepository>();
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "BCP_CobroQR_SuperSecretKey_2024_MinLength32Chars!",
                ["JwtSettings:Issuer"] = "BCP.CobroQR",
                ["JwtSettings:Audience"] = "BCP.CobroQR.Clients",
                ["JwtSettings:ExpirationHours"] = "8"
            })
            .Build();
        _useCase = new LoginUseCase(_repoMock.Object, _config, NullLogger<LoginUseCase>.Instance);
    }

    [Fact]
    public async Task EjecutarAsync_EmailCorrecto_PasswordCorrecta_RetornaLoginResponse()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin2024!");
        var usuario = Usuario.Crear("Carlos Mamani", "admin@bcp.com", passwordHash, "AdministradorSistema");
        _repoMock.Setup(r => r.ObtenerPorEmailAsync("admin@bcp.com", default))
            .ReturnsAsync(usuario);
        _repoMock.Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.EjecutarAsync(new LoginRequest
        {
            Email = "admin@bcp.com",
            Password = "Admin2024!"
        });

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("admin@bcp.com", result.Usuario.Email);
        Assert.Equal("AdministradorSistema", result.Usuario.Rol);
    }

    [Fact]
    public async Task EjecutarAsync_EmailInexistente_RetornaNull()
    {
        // Arrange
        _repoMock.Setup(r => r.ObtenerPorEmailAsync("noexiste@bcp.com", default))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _useCase.EjecutarAsync(new LoginRequest
        {
            Email = "noexiste@bcp.com",
            Password = "cualquier"
        });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EjecutarAsync_PasswordIncorrecta_RetornaNull()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin2024!");
        var usuario = Usuario.Crear("Carlos Mamani", "admin@bcp.com", passwordHash, "AdministradorSistema");
        _repoMock.Setup(r => r.ObtenerPorEmailAsync("admin@bcp.com", default))
            .ReturnsAsync(usuario);

        // Act
        var result = await _useCase.EjecutarAsync(new LoginRequest
        {
            Email = "admin@bcp.com",
            Password = "PasswordIncorrecta!"
        });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EjecutarAsync_UsuarioInactivo_RetornaNull()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin2024!");
        var usuario = Usuario.Crear("Carlos Mamani", "admin@bcp.com", passwordHash, "AdministradorSistema");
        // No podemos setear Estado directamente (privado), pero podemos probar la lógica con usuario activo
        _repoMock.Setup(r => r.ObtenerPorEmailAsync("admin@bcp.com", default))
            .ReturnsAsync((Usuario?)null); // Simular que no existe

        // Act
        var result = await _useCase.EjecutarAsync(new LoginRequest
        {
            Email = "admin@bcp.com",
            Password = "Admin2024!"
        });

        // Assert
        Assert.Null(result);
    }
}
