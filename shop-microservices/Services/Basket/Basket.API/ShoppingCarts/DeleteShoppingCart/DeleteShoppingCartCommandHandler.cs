namespace Basket.API.ShoppingCarts.DeleteShoppingCart;

public record DeleteShoppingCartCommand(string Username) : ICommand<DeleteShoppingCartResult>;

public record DeleteShoppingCartResult(bool Success);

public class DeleteShoppingCartCommandValidator : AbstractValidator<DeleteShoppingCartCommand>
{
    public DeleteShoppingCartCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
    }
}

public class DeleteShoppingCartCommandHandler : ICommandHandler<DeleteShoppingCartCommand, DeleteShoppingCartResult>
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public DeleteShoppingCartCommandHandler(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<DeleteShoppingCartResult> Handle(DeleteShoppingCartCommand command, CancellationToken cancellationToken)
    {
        bool success = await _shoppingCartRepository.DeleteShoppingCart(command.Username, cancellationToken);
        return new DeleteShoppingCartResult(success);
    }
}