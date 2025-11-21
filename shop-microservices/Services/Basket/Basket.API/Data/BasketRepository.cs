namespace Basket.API.Data;

public class BasketRepository : IBasketRepository
{
    private readonly IDocumentSession _session;

    public BasketRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<ShoppingCart> GetBasket(string username, CancellationToken cancellationToken = default)
    {
        var shoppingCart = await _session.LoadAsync<ShoppingCart>(username, cancellationToken);

        if (shoppingCart == null)
        {
            throw new ShoppingCartNotFoundException(username);
        }
        
        return shoppingCart;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        _session.Store(shoppingCart);
        await _session.SaveChangesAsync(cancellationToken);
        return shoppingCart;
    }

    public async Task<bool> DeleteBasket(string username, CancellationToken cancellationToken = default)
    {
        _session.Delete<ShoppingCart>(username);
        await _session.SaveChangesAsync(cancellationToken);
        return true;
    }
}