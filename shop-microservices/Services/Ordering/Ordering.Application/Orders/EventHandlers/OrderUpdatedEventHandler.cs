namespace Ordering.Application.Orders.EventHandlers;

public class OrderUpdatedEventHandler : INotificationHandler<OrderUpdatedEvent>
{
    private readonly ILogger<OrderUpdatedEventHandler> _logger;

    public OrderUpdatedEventHandler(ILogger<OrderUpdatedEventHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(OrderUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event handled: {DomainEvent}", domainEvent.GetType().Name);

        return Task.CompletedTask;
    }
}