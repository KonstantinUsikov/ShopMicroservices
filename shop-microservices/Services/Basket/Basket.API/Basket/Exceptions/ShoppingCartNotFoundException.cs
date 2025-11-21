namespace Basket.API.Basket.Exceptions;

public class ShoppingCartNotFoundException : NotFoundException
{
    public ShoppingCartNotFoundException(string username) : base("ShoppingCart", username)
    {
    }
}