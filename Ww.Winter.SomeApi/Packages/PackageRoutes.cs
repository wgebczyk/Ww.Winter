using Microsoft.AspNetCore.Mvc;

namespace Ww.Winter.Some.Packages;

public static class PackageRoutes
{
    public static void MapPackageRoutes(this WebApplication app)
    {
        var group = app.MapGroup("api/package")
            .WithTags("Package");

        group.MapGet("filter", QueryPackageFilter).WithName(nameof(QueryPackageFilter));
        group.MapGet("recipient/filter", QueryRecipientFilter).WithName(nameof(QueryRecipientFilter));
    }

    private static async Task<IList<Package>> QueryPackageFilter(
        [AsParameters] PackageQueries.QueryPackagesFilter filterParams,
        [AsParameters] SortParams sortParams,
        [AsParameters] PaginationParams paginationParams,
        [FromServices] PackageQueries carQueries,
        CancellationToken cancellationToken
    )
    {
        //return await carQueries.QueryPackages(filterParams, sortParams, paginationParams, cancellationToken);
        throw new InvalidOperationException();
    }

    private static async Task<IList<Package>> QueryRecipientFilter(
        [AsParameters] PackageQueries.QueryRecipientPackagesFilter filterParams,
        [AsParameters] SortParams sortParams,
        [AsParameters] PaginationParams paginationParams,
        [FromServices] PackageQueries carQueries,
        CancellationToken cancellationToken
    )
    {
        //return await carQueries.QueryRecipientPackages(filterParams, sortParams, paginationParams, cancellationToken);
        throw new InvalidOperationException();
    }
}
