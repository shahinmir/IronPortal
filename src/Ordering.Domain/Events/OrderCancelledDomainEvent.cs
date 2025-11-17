using IronExchange.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace IronExchange.Ordering.Domain.Events;

public class OrderCancelledDomainEvent : INotification
{
    public Order Order { get; }

    public OrderCancelledDomainEvent(Order order)
    {
        Order = order;
    }
}

