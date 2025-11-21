namespace Basket.API.Exceptions;

public class ShoppingCartNotFoundException : NotFoundException
{
    public ShoppingCartNotFoundException(string username) : base("ShoppingCart", username)
    {
    }
}