using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data;

public class CachedBasketRepository : IBasketRepository
{
    private readonly IBasketRepository _basketRepository;
    private readonly IDistributedCache _cache;

    public CachedBasketRepository(IBasketRepository basketRepository, IDistributedCache cache)
    {
        _basketRepository = basketRepository;
        _cache = cache;
    }

    public async Task<ShoppingCart> GetBasket(string username, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await _cache.GetStringAsync(username, cancellationToken);
        if (!string.IsNullOrEmpty(cachedBasket))
        {
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;
        }
        
        var basket = await _basketRepository.GetBasket(username, cancellationToken);
        
        await _cache.SetStringAsync(basket.Username, JsonSerializer.Serialize(basket), cancellationToken);
        
        return basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart shoppingCart, CancellationToken cancellationToken = default)
    {
        var basket = await _basketRepository.StoreBasket(shoppingCart, cancellationToken);
        
        await _cache.SetStringAsync(shoppingCart.Username, JsonSerializer.Serialize(shoppingCart), cancellationToken);
        
        return basket;
    }

    public async Task<bool> DeleteBasket(string username, CancellationToken cancellationToken = default)
    {
        var success = await _basketRepository.DeleteBasket(username, cancellationToken);
        
        await _cache.RemoveAsync(username, cancellationToken);
        
        return success;
    }
}