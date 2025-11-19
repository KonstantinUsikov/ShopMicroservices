namespace Basket.API.Models;

public class ShoppingCart
{
    public string Username { get; set; } = string.Empty;
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCart(string username)
    {
        Username = username;
    }
    
    public ShoppingCart()
    {
    }
}