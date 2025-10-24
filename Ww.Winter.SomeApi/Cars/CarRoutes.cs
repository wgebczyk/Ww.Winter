using Microsoft.AspNetCore.Mvc;

namespace Ww.Winter.Some.Cars;

public static class CarRoutes
{
    public static void MapCarRoutes(this WebApplication app)
    {
        var group = app.MapGroup("api/car")
            .WithTags("Car");

        group.MapGet("filter", QueryCarFilter);
    }

    private static async Task<IList<Car>> QueryCarFilter(
        [AsParameters] CarQueries.QueryCarsFilter filterParams,
        [AsParameters] SortParams sortParams,
        [AsParameters] PaginationParams paginationParams,
        [FromServices] CarQueries carQueries,
        CancellationToken cancellationToken
    )
    {
        return await carQueries.QueryCars(filterParams, sortParams, paginationParams, cancellationToken);
    }
}
