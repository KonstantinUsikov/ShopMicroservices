namespace Basket.API.ShoppingCarts.CheckoutShoppingCart;

public record CheckoutShoppingCartRequest(ShoppingCartCheckoutDto ShoppingCartCheckoutDto);

public record CheckoutShoppingCartResponse(bool Success);

public class CheckoutShoppingCartEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutShoppingCartRequest request, ISender sender) =>
            {
                var command = request.Adapt<CheckoutShoppingCartCommand>();
        
                var result = await sender.Send(command);
        
                var response = result.Adapt<CheckoutShoppingCartCommand>();
        
                return Results.Ok(response);
            })
            .WithName("CheckoutShoppingCart")
            .Produces<CheckoutShoppingCartResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Checkout Shopping Cart")
            .WithDescription("Checkout Shopping Cart");
    }
}