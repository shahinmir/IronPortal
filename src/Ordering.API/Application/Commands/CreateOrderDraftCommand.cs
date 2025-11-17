namespace IronExchange.Ordering.API.Application.Commands;
using IronExchange.Ordering.API.Application.Models;

public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items) : IRequest<OrderDraftDTO>;
