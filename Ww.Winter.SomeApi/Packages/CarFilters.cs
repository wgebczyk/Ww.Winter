namespace Ww.Winter.Some.Packages;

[QueryFilter(typeof(Package), typeof(PackageQueries.QueryPackagesFilter))]
[QueryFilter(typeof(Package), typeof(PackageQueries.QueryRecipientPackagesFilter))]
public static partial class PackageFilters
{
}
