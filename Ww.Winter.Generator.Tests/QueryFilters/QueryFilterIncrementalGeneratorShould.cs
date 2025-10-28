using FluentAssertions;
using Microsoft.CodeAnalysis;
using Ww.Winter.Generator.QueryFilters;

namespace Ww.Winter.Generator.Tests.QueryFilters;

public sealed class QueryFilterIncrementalGeneratorShould
{
    [Fact]
    public void HandleKitchenSinkExample()
    {
        var source0 =
        """
        namespace Ww.Winter.Generator.Tests;

        public partial class BookQueries
        {
            public sealed record QueryBooksFilter
            {
                public string? TitlePrefix { get; init; }
                public string? TitleFragment { get; init; }
                public string? Isbn { get; init; }
                public string? AuthorFragment { get; init; }
                public string? CategoryFragment { get; init; }
                public int? PageCountFrom { get; init; }
                public int? PageCountTo { get; init; }
            }
        }
        """;
        var source1 =
        """
        using Ww.Winter;
        using Ww.Winter.Some.Books;

        namespace Ww.Winter.Generator.Tests;

        [QueryFilter(typeof(Ww.Winter.Some.Book), typeof(BookQueries.QueryBooksFilter))]
        public static partial class BookFilters
        {
        }
        """;

        var driver = CodeGeneration.DriveSource(new QueryFilterIncrementalGenerator(), [source0, source1], additionalMetadata: [
            MetadataReference.CreateFromFile(typeof(Ww.Winter.Some.Book).Assembly.Location)
        ]);
        var result = driver.GetRunResult();

        var expectedLines =
            """
            using System.Linq;

            #nullable enable

            namespace Ww.Winter.Generator.Tests;

            partial class BookFilters
            {
                public static IQueryable<Book> ApplyFilter(
                    this IQueryable<Book> query,
                    BookQueries.QueryBooksFilter filter)
                {
                    if (filter.TitlePrefix is not null)
                    {
                        query = query.Where(e => e.Title.StartsWith(filter.TitlePrefix));
                    }
                    if (filter.TitleFragment is not null)
                    {
                        query = query.Where(e => e.Title.Contains(filter.TitleFragment));
                    }
                    if (filter.Isbn is not null)
                    {
                        query = query.Where(e => e.Isbn == filter.Isbn);
                    }
                    if (filter.AuthorFragment is not null)
                    {
                        query = query.Where(e => e.Author.Contains(filter.AuthorFragment));
                    }
                    if (filter.CategoryFragment is not null)
                    {
                        query = query.Where(e => e.Category.Contains(filter.CategoryFragment));
                    }
                    if (filter.PageCountFrom is not null)
                    {
                        query = query.Where(e => e.PageCount >= filter.PageCountFrom);
                    }
                    if (filter.PageCountTo is not null)
                    {
                        query = query.Where(e => e.PageCount <= filter.PageCountTo);
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
