using IronExchange.EventBus.Events;

namespace IronExchange.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}
