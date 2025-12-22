using MassTransit;
using Microsoft.FeatureManagement;

namespace Ordering.Application.Orders.EventHandlers.Domain;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(IPublishEndpoint publishEndpoint, IFeatureManager featureManager, 
        ILogger<OrderCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _featureManager = featureManager;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event handled: {DomainEvent}", domainEvent.GetType().Name);

        if (await _featureManager.IsEnabledAsync("OrderFulfillment"))
        {
            var orderCreatedIntegrationEvent = domainEvent.Order.ToOrderDto();
            await _publishEndpoint.Publish(orderCreatedIntegrationEvent, cancellationToken);
        }
    }
}