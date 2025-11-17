using IronExchange.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace IronExchange.Ordering.Domain.Events;

public class OrderShippedDomainEvent : INotification
{
    public Order Order { get; }

    public OrderShippedDomainEvent(Order order)
    {
        Order = order;
    }
}
