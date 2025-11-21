namespace Basket.API.ShoppingCarts.GetShoppingCart;

//public record GetShoppingCartRequest();

public record GetShoppingCartResponse(ShoppingCart ShoppingCart);

public class GetShoppingCartEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{username}", async (string username, ISender sender) =>
            {
                var query = new GetShoppingCartQuery(username);

                var result = await sender.Send(query);

                var response = result.Adapt<GetShoppingCartResponse>();

                return Results.Ok(response);
            })
            .WithName("GetShoppingCart")
            .Produces<GetShoppingCartResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Shopping Cart")
            .WithDescription("Get Shopping Cart");
    }
}