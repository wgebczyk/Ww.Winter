using FluentAssertions;
using Ww.Winter.Generator.Queries;

namespace Ww.Winter.Generator.Tests.Queries;

public sealed class QueryIncrementalGeneratorShould
{
    [Fact]
    public void HandleKitchenSinkExample()
    {
        var source =
        @"
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.EntityFrameworkCore;

        namespace Ww.Winter.Generator.Tests;

        public sealed record SortParams
        {
            public string? Sort { get; init; }
        }
        public sealed record PaginationParams
        {
            public int? Skip { get; init; }
            public int? Take { get; init; }
        }
        public class ApiDbContext
        {
            public DbSet<Car> Cars { get; } = null!;
        }

        public class Car
        {
            public required int Id { get; set; }
            public required string Model { get; set; }
            public required string Manufacturer { get; set; }
            public required string Color { get; set; }
            public required string Vin { get; set; }
            public required int Year { get; set; }
        }

        public partial class CarQueries
        {
            private readonly ApiDbContext dbContext;

            public CarQueries(ApiDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public sealed record QueryCarsFilter
            {
                public string? ManufacturerPrefix { get; init; }
                public string? ManufacturerFragment { get; init; }
                public string? ModelPrefix { get; init; }
                public int? Year { get; init; }
                public int? YearFrom { get; init; }
                public int? YearTo { get; init; }
                public string? Vin { get; init; }
                public string? VinPrefix { get; init; }
                public bool OnlySpecialFlag { get; init; }
            }
            [Query(typeof(Car))]
            public partial Task<IList<QueryCarsSummaryDto>> QueryCars(
                QueryCarsFilter filter,
                SortParams sort,
                PaginationParams pagination,
                CancellationToken cancellationToken
            );
            public sealed record QueryCarsSummaryDto(
                string Model,
                int Year
            );

            private IQueryable<Car> GetBaseQuery() {
                return dbContext.Cars.IgnoreQueryFilters();
            }
            public QueryCarsSummaryDto QueryCarsProjectTo(Car entity) {
                return new QueryCarsSummaryDto(
                    entity.Model,
                    entity.Year
                );
            }
            private IQueryable<Car> ApplyOnlySpecialFlag(IQueryable<Car> query, bool value) {
                if (!value) { return query; }
                return query.Where(x => x.Color == ""Special"");
            }
        }
        ";

        var driver = CodeGeneration.DriveSource(new QueryIncrementalGenerator(), source);
        var result = driver.GetRunResult();

        var expectedLines =
            """
            namespace Ww.Winter.Generator.Tests;

            partial class CarQueries
            {
                public partial async Task<IList<QueryCarsSummaryDto>> QueryCars(
                    CarQueries.QueryCarsFilter filter,
                    SortParams sort,
                    PaginationParams pagination,
                    CancellationToken cancellationToken)
                {
                    var query = this.dbContext.Cars
                        .AsNoTracking()
                        .TagWith("QueryCars");

                    if (filter.ManufacturerPrefix is not null)
                    {
                        query = query.Where(e => e.Manufacturer.StartsWith(filter.ManufacturerPrefix));
                    }
                    if (filter.ManufacturerFragment is not null)
                    {
                        query = query.Where(e => e.Manufacturer.Contains(filter.ManufacturerFragment));
                    }
                    if (filter.ModelPrefix is not null)
                    {
                        query = query.Where(e => e.Model.StartsWith(filter.ModelPrefix));
                    }
                    if (filter.Year is not null)
                    {
                        query = query.Where(e => e.Year == filter.Year);
                    }
                    if (filter.YearFrom is not null)
                    {
                        query = query.Where(e => e.Year >= filter.YearFrom);
                    }
                    if (filter.YearTo is not null)
                    {
                        query = query.Where(e => e.Year <= filter.YearTo);
                    }
                    if (filter.Vin is not null)
                    {
                        query = query.Where(e => e.Vin == filter.Vin);
                    }
                    if (filter.VinPrefix is not null)
                    {
                        query = query.Where(e => e.Vin.StartsWith(filter.VinPrefix));
                    }
                    query = ApplyOnlySpecialFlag(query, filter.OnlySpecialFlag);

                    query = ApplySort(query, sort);
                    query = ApplyPagination(query, pagination);

                    return await query
                        .Select(QueryCarsProjectTo)
                        .ToListAsync(cancellationToken);
                }

                private IQueryable<Car> ApplySort(IQueryable<Car> query, SortParams sort)
                {
                    foreach (var property in sort.Properties)
                    {
                        var propertyName = property.PropertyName.ToLowerInvariant();
                        var direction = property.Direction;
                        switch (propertyName)
                        {
                            case "id":
                                query = ApplySort(query, o => o.Id, direction == ListSortDirection.Descending);
                                break;
                            case "model":
                                query = ApplySort(query, o => o.Model, direction == ListSortDirection.Descending);
                                break;
                            case "manufacturer":
                                query = ApplySort(query, o => o.Manufacturer, direction == ListSortDirection.Descending);
                                break;
                            case "color":
                                query = ApplySort(query, o => o.Color, direction == ListSortDirection.Descending);
                                break;
                            case "vin":
                                query = ApplySort(query, o => o.Vin, direction == ListSortDirection.Descending);
                                break;
                            case "year":
                                query = ApplySort(query, o => o.Year, direction == ListSortDirection.Descending);
                                break;
                        }
                    }
                    return query;
                }
            }
            """
            .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("{") && !x.StartsWith("}") && !x.StartsWith("break"))
            .Order()
            .Distinct()
            .ToList();

        var generatedSource = result.Results.First().GeneratedSources.First().SourceText.ToString();
        foreach (var expectedLine in expectedLines)
        {
            generatedSource.Should().Contain(expectedLine);
        }
    }
}
