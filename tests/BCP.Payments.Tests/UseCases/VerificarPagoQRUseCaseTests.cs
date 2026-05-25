namespace BCP.Payments.Tests.UseCases;

using BCP.Payments.API.Application.DTOs;
using BCP.Payments.API.Application.UseCases;
using BCP.Payments.API.Domain.Interfaces;
using BCP.Payments.API.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

public class VerificarPagoQRUseCaseTests
{
    private readonly Mock<ITransaccionRepository> _repoMock = new();
    private readonly Mock<IBCPExternalService> _bcpMock = new();
    private readonly Mock<IEventPublisher> _publisherMock = new();
    private readonly VerificarPagoQRUseCase _useCase;

    public VerificarPagoQRUseCaseTests()
    {
        _useCase = new VerificarPagoQRUseCase(
            _repoMock.Object, _bcpMock.Object,
            _publisherMock.Object, NullLogger<VerificarPagoQRUseCase>.Instance);
    }

    private static VerificarPagoRequest CrearRequest(string referencia = "REF-001") => new()
    {
        SucursalId = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        Monto = 100m,
        ReferenciaCliente = referencia
    };

    [Fact]
    public async Task EjecutarAsync_PagoConfirmado_RetornaEstadoConfirmada()
    {
        // Arrange
        var request = CrearRequest();
        _repoMock.Setup(r => r.ObtenerPorReferenciaClienteAsync(request.ReferenciaCliente, default))
            .ReturnsAsync((Transaccion?)null);
        _repoMock.Setup(r => r.GuardarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.ActualizarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _bcpMock.Setup(b => b.VerificarPagoAsync(request.ReferenciaCliente, request.Monto, request.SucursalId, default))
            .ReturnsAsync(new RespuestaBCP("EXT-001", "Confirmado", "AUTH-123", "Pago confirmado"));
        _publisherMock.Setup(p => p.PublicarAsync(It.IsAny<BCP.Payments.API.Domain.Events.IDomainEvent>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.EjecutarAsync(request);

        // Assert
        Assert.Equal("Confirmada", result.Estado);
        Assert.False(result.EsIdempotente);
        Assert.Equal("AUTH-123", result.CodigoAutorizacion);
    }

    [Fact]
    public async Task EjecutarAsync_ReferenciaExistente_RetornaResultadoIdempotente()
    {
        // Arrange
        var request = CrearRequest("REF-EXISTENTE");
        var transaccionExistente = Transaccion.Iniciar(request.SucursalId, request.UsuarioId, 100m, "REF-EXISTENTE");
        _repoMock.Setup(r => r.ObtenerPorReferenciaClienteAsync("REF-EXISTENTE", default))
            .ReturnsAsync(transaccionExistente);

        // Act
        var result = await _useCase.EjecutarAsync(request);

        // Assert
        Assert.True(result.EsIdempotente);
        _bcpMock.Verify(b => b.VerificarPagoAsync(It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_BCPTimeout_RegistraError()
    {
        // Arrange
        var request = CrearRequest("REF-TIMEOUT");
        _repoMock.Setup(r => r.ObtenerPorReferenciaClienteAsync(request.ReferenciaCliente, default))
            .ReturnsAsync((Transaccion?)null);
        _repoMock.Setup(r => r.GuardarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.ActualizarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _bcpMock.Setup(b => b.VerificarPagoAsync(It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        _publisherMock.Setup(p => p.PublicarAsync(It.IsAny<BCP.Payments.API.Domain.Events.IDomainEvent>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.EjecutarAsync(request);

        // Assert
        Assert.Equal("Error", result.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_PagoRechazado_RetornaEstadoRechazada()
    {
        // Arrange
        var request = CrearRequest("REF-RECHAZADO");
        _repoMock.Setup(r => r.ObtenerPorReferenciaClienteAsync(request.ReferenciaCliente, default))
            .ReturnsAsync((Transaccion?)null);
        _repoMock.Setup(r => r.GuardarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.ActualizarAsync(It.IsAny<Transaccion>(), default)).Returns(Task.CompletedTask);
        _bcpMock.Setup(b => b.VerificarPagoAsync(request.ReferenciaCliente, request.Monto, request.SucursalId, default))
            .ReturnsAsync(new RespuestaBCP("EXT-002", "Rechazado", null, "Fondos insuficientes"));
        _publisherMock.Setup(p => p.PublicarAsync(It.IsAny<BCP.Payments.API.Domain.Events.IDomainEvent>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.EjecutarAsync(request);

        // Assert
        Assert.Equal("Rechazada", result.Estado);
    }
}
