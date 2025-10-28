using FluentAssertions;
using Microsoft.CodeAnalysis;
using Ww.Winter.Generator.QueryableFilters;

namespace Ww.Winter.Generator.Tests.QueryableFilters;

public sealed class QueryableFilterIncrementalGeneratorShould
{
    [Fact]
    public void HandleKitchenSinkExample()
    {
        var source =
        @"""
        namespace Ww.Winter.Generator.Tests;

        public partial class BookQueries
        {
            [QueryableFilter(typeof(Ww.Winter.Some.Book))]
            public sealed partial class QueryBooksFilter
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

        var driver = CodeGeneration.DriveSource(new QueryableFilterIncrementalGenerator(), source, additionalMetadata: [
            MetadataReference.CreateFromFile(typeof(Ww.Winter.Some.Book).Assembly.Location)
        ]);
        var result = driver.GetRunResult();

        var expectedLines =
            """
            using System.Linq;
            using Ww.Winter.Some;

            #nullable enable

            namespace Ww.Winter.Generator.Tests;

            partial class BookQueries
            {
                partial class QueryBooksFilter
                {
                    public IQueryable<Book> ApplyFilter(IQueryable<Book> query)
                    {
                        if (this.TitlePrefix is not null)
                        {
                            query = query.Where(e => e.Title.StartsWith(this.TitlePrefix));
                        }
                        if (this.TitleFragment is not null)
                        {
                            query = query.Where(e => e.Title.Contains(this.TitleFragment));
                        }
                        if (this.Isbn is not null)
                        {
                            query = query.Where(e => e.Isbn == this.Isbn);
                        }
                        if (this.AuthorFragment is not null)
                        {
                            query = query.Where(e => e.Author.Contains(this.AuthorFragment));
                        }
                        if (this.CategoryFragment is not null)
                        {
                            query = query.Where(e => e.Category.Contains(this.CategoryFragment));
                        }
                        if (this.PageCountFrom is not null)
                        {
                            query = query.Where(e => e.PageCount >= this.PageCountFrom);
                        }
                        if (this.PageCountTo is not null)
                        {
                            query = query.Where(e => e.PageCount <= this.PageCountTo);
                        }

                        return query;
                    }
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
