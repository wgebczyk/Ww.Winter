using Microsoft.AspNetCore.Mvc;

namespace Ww.Winter.Some.Packages;

public partial class PackageQueries
{
    private readonly PackageDbContext dbContext;

    public PackageQueries(PackageDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [QueryableFilter(typeof(Package))]
    public sealed partial record QueryPackagesFilter
    {
        public int? Id { get; init; }
        public string? Number { get; init; }
        public string? NumberPrefix { get; init; }
        public string? SenderNameFragment { get; init; }
        public string? SenderPostalCode { get; init; }
        public string? SenderPostalCodePrefix { get; init; }
        public string? RecipientNameFragment { get; init; }
        public string? RecipientPostalCode { get; init; }
        public string? RecipientPostalCodePrefix { get; init; }
    }
    //[Query(typeof(Package))]
    //public partial Task<IList<Package>> QueryPackages(
    //    QueryPackagesFilter filter,
    //    SortParams sort,
    //    PaginationParams pagination,
    //    CancellationToken cancellationToken
    //);

    [QueryableFilter(typeof(Package))]
    public sealed partial record QueryRecipientPackagesFilter
    {
        [FromQuery(Name = "name")]
        public string? RecipientNameFragment { get; init; }
        [FromQuery(Name = "address")]
        public string? RecipientAddressFragment { get; init; }
        [FromQuery(Name = "postal-code")]
        public string? RecipientPostalCode { get; init; }
        [FromQuery(Name = "posta-code-starts-with")]
        public string? RecipientPostalCodePrefix { get; init; }
    }
    //[Query(typeof(Package))]
    //public partial Task<IList<Package>> QueryRecipientPackages(
    //    QueryRecipientPackagesFilter filter,
    //    SortParams sort,
    //    PaginationParams pagination,
    //    CancellationToken cancellationToken
    //);
}
