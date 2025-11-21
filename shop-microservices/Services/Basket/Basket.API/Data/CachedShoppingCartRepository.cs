using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data;

public class CachedShoppingCartRepository : IShoppingCartRepository
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IDistributedCache _cache;

    public CachedShoppingCartRepository(IShoppingCartRepository shoppingCartRepository, IDistributedCache cache)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _cache = cache;
    }

    public async Task<ShoppingCart> GetShoppingCart(string username, CancellationToken cancellationToken = default)
    {
        var cachedShoppingCart = await _cache.GetStringAsync(username, cancellationToken);
        if (!string.IsNullOrEmpty(cachedShoppingCart))
        {
            return JsonSerializer.Deserialize<ShoppingCart>(cachedShoppingCart)!;
        }
        
        var shoppingCart = await _shoppingCartRepository.GetShoppingCart(username, cancellationToken);
        
        await _cache.SetStringAsync(shoppingCart.Username, JsonSerializer.Serialize(shoppingCart), cancellationToken);
        
        return shoppingCart;
    }

    public async Task<ShoppingCart> StoreShoppingCart(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        var cart = await _shoppingCartRepository.StoreShoppingCart(shoppingCart, cancellationToken);
        
        await _cache.SetStringAsync(shoppingCart.Username, JsonSerializer.Serialize(shoppingCart), cancellationToken);
        
        return cart;
    }

    public async Task<bool> DeleteShoppingCart(string username, CancellationToken cancellationToken = default)
    {
        var success = await _shoppingCartRepository.DeleteShoppingCart(username, cancellationToken);
        
        await _cache.RemoveAsync(username, cancellationToken);
        
        return success;
    }
}