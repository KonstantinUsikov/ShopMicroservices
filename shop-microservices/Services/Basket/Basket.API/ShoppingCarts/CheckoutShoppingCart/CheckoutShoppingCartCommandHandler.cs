using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Basket.API.ShoppingCarts.CheckoutShoppingCart;

public record CheckoutShoppingCartCommand(ShoppingCartCheckoutDto ShoppingCartCheckoutDto) : ICommand<CheckoutShoppingCartResult>;

public record CheckoutShoppingCartResult(bool Success);

public class DeleteShoppingCartCommandValidator : AbstractValidator<CheckoutShoppingCartCommand>
{
    public DeleteShoppingCartCommandValidator()
    {
        RuleFor(x => x.ShoppingCartCheckoutDto).NotNull().WithMessage("ShoppingCartCheckoutDto can't be null");
        RuleFor(x => x.ShoppingCartCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required");
    }
}

public class CheckoutShoppingCartCommandHandler : ICommandHandler<CheckoutShoppingCartCommand, CheckoutShoppingCartResult>
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutShoppingCartCommandHandler(IShoppingCartRepository shoppingCartRepository, IPublishEndpoint publishEndpoint)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CheckoutShoppingCartResult> Handle(CheckoutShoppingCartCommand command, CancellationToken cancellationToken)
    {
        var shoppingCart = await _shoppingCartRepository.GetShoppingCart(command.ShoppingCartCheckoutDto.UserName, cancellationToken);
        if (shoppingCart == null)
        {
            return new CheckoutShoppingCartResult(false);
        }

        var eventMessage = command.ShoppingCartCheckoutDto.Adapt<ShoppingCartCheckoutEvent>();
        eventMessage.TotalPrice = shoppingCart.TotalPrice;

        await _publishEndpoint.Publish(eventMessage, cancellationToken);

        await _shoppingCartRepository.DeleteShoppingCart(command.ShoppingCartCheckoutDto.UserName, cancellationToken);

        return new CheckoutShoppingCartResult(true);
    }
}