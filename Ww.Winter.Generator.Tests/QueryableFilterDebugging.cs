using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ww.Winter.Generator.QueryableFilters;

namespace Ww.Winter.Generator.Tests;

public class QueryableFilterDebugging
{
    [Fact]
    public void DebugIt()
    {
        var source1 =
        """
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
        var source2 =
        """
        namespace Ww.Winter.Generator.Tests;
        """;

        var driver = DriveQueryableFilter(source1, source2);
        var result = driver.GetRunResult();
    }

    public static GeneratorDriver DriveQueryableFilter(string source1, string source2)
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
                MetadataReference.CreateFromFile(typeof(QueryableFilterAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Ww.Winter.Some.Book).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );

        var diagnostics = compilation.GetDiagnostics();
        diagnostics.Should().BeEmpty();

        var generator = new QueryableFilterIncrementalGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return driver;
    }
}
