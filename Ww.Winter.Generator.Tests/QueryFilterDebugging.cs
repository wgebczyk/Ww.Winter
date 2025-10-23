using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ww.Winter.Generator.QueryFilters;

namespace Ww.Winter.Generator.Tests;

public class QueryFilterDebugging
{
    [Fact]
    public void DebugIt()
    {
        var source1 =
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
        var source2 =
        """
        using Ww.Winter;
        using Ww.Winter.Some.Books;

        namespace Ww.Winter.Generator.Tests;

        [QueryFilter(typeof(Ww.Winter.Some.Books.Book), typeof(BookQueries.QueryBooksFilter))]
        public static partial class BookFilters
        {
        }
        """;

        var driver = DriveQueryFilter(source1, source2);
        var result = driver.GetRunResult();
    }

    public static GeneratorDriver DriveQueryFilter(string source1, string source2)
    {
        SyntaxTree syntaxTree1 = CSharpSyntaxTree.ParseText(source1, path: "source1.cs");
        SyntaxTree syntaxTree2 = CSharpSyntaxTree.ParseText(source2, path: "source2.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Ww.Winter.Generator.Tests__",
            syntaxTrees: [syntaxTree1, syntaxTree2],
            references: [
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(QueryFilterAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Ww.Winter.Some.Book).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

        var generator = new QueryFilterIncrementalGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return driver;
    }
}
