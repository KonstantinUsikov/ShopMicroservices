namespace Catalog.API.Products.GetProductsByCategory;

//public record GetProductsByCategoryEndpointRequest();

public record GetProductsByCategoryEndpointResponse(IEnumerable<Product> Products);

public class GetProductsByCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", async (string category, ISender sender) =>
            {
                var command = new GetProductsByCategoryQuery(category);

                var result = await sender.Send(command);

                var response = result.Adapt<GetProductsByCategoryEndpointResponse>();

                return Results.Ok(response);
            })
            .WithName("GetProductsByCategory")
            .Produces<GetProductsByCategoryEndpointResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Products By Category")
            .WithDescription("Get Products By Category");
    }
}