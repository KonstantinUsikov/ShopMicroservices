namespace Basket.API.ShoppingCarts.GetShoppingCart;

public record GetShoppingCartQuery(string Username) : IQuery<GetShoppingCartResult>;

public record GetShoppingCartResult(ShoppingCart ShoppingCart);

public class GetShoppingCartQueryHandler : IQueryHandler<GetShoppingCartQuery, GetShoppingCartResult>
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public GetShoppingCartQueryHandler(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<GetShoppingCartResult> Handle(GetShoppingCartQuery query, CancellationToken cancellationToken)
    {
        var shoppingCart = await _shoppingCartRepository.GetShoppingCart(query.Username, cancellationToken);
        return new GetShoppingCartResult(shoppingCart);
    }
}