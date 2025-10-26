using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ww.Winter.Generator.Queries;

namespace Ww.Winter.Generator.Tests;

public class QueryDebugging
{
    [Fact]
    public void DebugIt()
    {
        var source =
        @"
        using Ww.Winter;

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

        public class Car
        {
            public int Id { get; set; }
            public string Model { get; set; }
            public string Manufacturer { get; set; }
            public string Color { get; set; }
            public string Vin { get; set; }
            public int Year { get; set; }
        }

        public class Package
        {
            public int Id { get; set; }
            public string Number { get; set; }

            public string SenderName { get; set; }
            public string SenderAddress { get; set; }
            public string SenderPostalCode { get; set; }

            public string RecipientName { get; set; }
            public string RecipientAddress { get; set; }
            public string RecipientPostalCode { get; set; }
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
            public partial Task<IList<Car>> QueryCars(
                QueryCarsFilter filter,
                SortParams sort,
                PaginationParams pagination,
                CancellationToken cancellationToken
            );
            public sealed record QueryCarsSummaryDto(
                string Model,
                string Year
            );

            private IQueryable<Car> GetBaseQuery() {
                return dbContext.Cars.IgnoreQueryFilters();
            }
            public CarSummaryDto QueryCarsProjectTo(Car entity) {
                return new CarSummaryDto(
                    entity.Model,
                    entity.Year
                );
            }
            private IQueryable<Book> ApplyOnlySpecialFlag(IQueryable<Book> query, bool value) {
                if (!value) { return query; }
                return query.Where(b => b.Color == ""Special"");
            }
        }

        public partial class PackageQueries
        {
            private readonly ApiDbContext dbContext;

            public PackageQueries(ApiDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public sealed record QueryPackagesFilter
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
            [Query(typeof(Package))]
            public partial Task<IList<Package>> QueryPackages(
                QueryPackagesFilter filter,
                SortParams sort,
                PaginationParams pagination,
                CancellationToken cancellationToken
            );

            public sealed record QueryRecipientPackagesFilter
            {
                [FromQuery(Name = ""name"")]
                public string? RecipientNameFragment { get; init; }
                [FromQuery(Name = ""address"")]
                public string? RecipientAddressFragment { get; init; }
                [FromQuery(Name = ""postal-code"")]
                public string? RecipientPostalCode { get; init; }
                [FromQuery(Name = ""posta-code-starts-with"")]
                public string? RecipientPostalCodePrefix { get; init; }
            }
            [Query(typeof(Package))]
            public partial Task<IList<Package>> QueryRecipientPackages(
                QueryRecipientPackagesFilter filter,
                SortParams sort,
                PaginationParams pagination,
                CancellationToken cancellationToken
            );
        }
        ";

        var driver = DriveQuery(source);
        var result = driver.GetRunResult();
    }

    public static GeneratorDriver DriveQuery(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, path: "source1.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Ww.Winter.Generator.Tests__",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(QueryAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ]
        );

        var diagnostics = compilation.GetDiagnostics();
        //diagnostics.Should().BeEmpty();

        var generator = new QueryIncrementalGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return driver;
    }
}
