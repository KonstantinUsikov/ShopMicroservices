using Discount.Grpc;

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
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

    public StoreShoppingCartCommandHandler(IShoppingCartRepository shoppingCartRepository, 
        DiscountProtoService.DiscountProtoServiceClient discountProtoService)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _discountProtoService = discountProtoService;
    }

    public async Task<StoreShoppingCartResult> Handle(StoreShoppingCartCommand command, CancellationToken cancellationToken)
    {
        await DeductDiscount(command.ShoppingCart, cancellationToken);
        await _shoppingCartRepository.StoreShoppingCart(command.ShoppingCart, cancellationToken);
        return new StoreShoppingCartResult(command.ShoppingCart.Username);
    }

    private async Task DeductDiscount(ShoppingCart shoppingCart, CancellationToken cancellationToken)
    {
        foreach (var item in shoppingCart.Items)
        {
            var coupon = await _discountProtoService.GetDiscountAsync(new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: cancellationToken);
            item.Price -= coupon.Amount;
        }
    }
}