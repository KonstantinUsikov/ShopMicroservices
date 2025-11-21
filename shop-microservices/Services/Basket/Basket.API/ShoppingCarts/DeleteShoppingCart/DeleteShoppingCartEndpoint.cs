namespace Basket.API.ShoppingCarts.DeleteShoppingCart;

//public record DeleteShoppingCartRequest(string Username);

public record DeleteShoppingCartResponse(bool Success);

public class DeleteShoppingCartEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{username}", async (string username, ISender sender) =>
            {
                var command = new DeleteShoppingCartCommand(username);

                var result = await sender.Send(command);

                var response = result.Adapt<DeleteShoppingCartResponse>();

                return Results.Ok(response);
            })
            .WithName("DeleteShoppingCart")
            .Produces<DeleteShoppingCartResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Shopping Cart")
            .WithDescription("Delete Shopping Cart");
    }
}