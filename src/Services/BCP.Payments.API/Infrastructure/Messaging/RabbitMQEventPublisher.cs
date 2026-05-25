namespace BCP.Payments.API.Infrastructure.Messaging;

using BCP.Payments.API.Domain.Events;
using BCP.Payments.API.Domain.Interfaces;
using MassTransit;

public class RabbitMQEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RabbitMQEventPublisher> _logger;

    public RabbitMQEventPublisher(IPublishEndpoint publishEndpoint, ILogger<RabbitMQEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublicarAsync<T>(T evento, CancellationToken ct = default) where T : IDomainEvent
    {
        try
        {
            await _publishEndpoint.Publish(evento, ct);
            _logger.LogInformation("Evento publicado: {TipoEvento} id={EventId}", typeof(T).Name, evento.EventId);
        }
        catch (Exception ex)
        {
            // No fallar el flujo principal si RabbitMQ no está disponible
            _logger.LogError(ex, "Error publicando evento {TipoEvento}: {Mensaje}", typeof(T).Name, ex.Message);
        }
    }
}
