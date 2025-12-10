namespace Ordering.Domain.Models;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; } = Guid.Empty;
    public Guid ProductId { get; private set; } = Guid.Empty;
    public int Quantity { get; private set; } = 0;
    public decimal Price { get; private set; } = 0;

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal price)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
}