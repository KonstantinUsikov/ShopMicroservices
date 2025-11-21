namespace Basket.API.Data;

public interface IShoppingCartRepository
{
    Task<ShoppingCart> GetShoppingCart(string username, CancellationToken cancellationToken = default);
    Task<ShoppingCart> StoreShoppingCart(ShoppingCart shoppingCart, CancellationToken cancellationToken = default);
    Task<bool> DeleteShoppingCart(string username, CancellationToken cancellationToken = default);
}