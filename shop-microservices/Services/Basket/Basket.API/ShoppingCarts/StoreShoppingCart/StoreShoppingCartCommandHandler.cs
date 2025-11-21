namespace Basket.API.ShoppingCarts.StoreShoppingCart;

public record StoreShoppingCartCommand(ShoppingCart ShoppingCart) : ICommand<StoreShoppingCartResult>;

public record StoreShoppingCartResult(string Username);

public class StoreShoppingCartCommandValidator : AbstractValidator<StoreShoppingCartCommand>
{
    public StoreShoppingCartCommandValidator()
    {
        RuleFor(x => x.ShoppingCart).NotNull().WithMessage("ShoppingCart is required");
        RuleFor(x => x.ShoppingCart.Username).NotEmpty().WithMessage("Username is required");
    }
}

public class StoreShoppingCartCommandHandler : ICommandHandler<StoreShoppingCartCommand, StoreShoppingCartResult>
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public StoreShoppingCartCommandHandler(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<StoreShoppingCartResult> Handle(StoreShoppingCartCommand command, CancellationToken cancellationToken)
    {
        await _shoppingCartRepository.StoreShoppingCart(command.ShoppingCart, cancellationToken);
        return new StoreShoppingCartResult(command.ShoppingCart.Username);
    }
}