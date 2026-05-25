namespace BCP.Payments.API.Domain.Interfaces;

using BCP.Payments.API.Domain.Events;

public interface IEventPublisher
{
    Task PublicarAsync<T>(T evento, CancellationToken ct = default) where T : IDomainEvent;
}
