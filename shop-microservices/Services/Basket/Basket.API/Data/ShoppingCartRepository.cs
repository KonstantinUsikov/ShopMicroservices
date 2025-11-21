namespace Basket.API.Data;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly IDocumentSession _session;

    public ShoppingCartRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<ShoppingCart> GetShoppingCart(string username, CancellationToken cancellationToken = default)
    {
        var shoppingCart = await _session.LoadAsync<ShoppingCart>(username, cancellationToken);

        if (shoppingCart == null)
        {
            throw new ShoppingCartNotFoundException(username);
        }
        
        return shoppingCart;
    }

    public async Task<ShoppingCart> StoreShoppingCart(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        _session.Store(shoppingCart);
        await _session.SaveChangesAsync(cancellationToken);
        return shoppingCart;
    }

    public async Task<bool> DeleteShoppingCart(string username, CancellationToken cancellationToken = default)
    {
        _session.Delete<ShoppingCart>(username);
        await _session.SaveChangesAsync(cancellationToken);
        return true;
    }
}