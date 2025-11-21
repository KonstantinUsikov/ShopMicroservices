namespace Basket.API.ShoppingCarts.StoreShoppingCart;

public record StoreShoppingCartRequest(ShoppingCart ShoppingCart);

public record StoreShoppingCartResponse(string Username);

public class StoreShoppingCartEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (StoreShoppingCartRequest request, ISender sender) =>
            {
                var command = request.Adapt<StoreShoppingCartCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<StoreShoppingCartResponse>();

                return Results.Created($"/basket/{response.Username}", response);
            })
            .WithName("StoreShoppingCart")
            .Produces<StoreShoppingCartResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Store Shopping Cart")
            .WithDescription("Store Shopping Cart");
    }
}