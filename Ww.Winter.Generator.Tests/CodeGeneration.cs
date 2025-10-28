using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Ww.Winter.Generator.Tests;

public static class CodeGeneration
{
    public static GeneratorDriver DriveSource(IIncrementalGenerator generator, string source, bool verifyNoErrors = false, MetadataReference[]? additionalMetadata = null)
    {
        return DriveSource(generator, [source], verifyNoErrors, additionalMetadata);
    }
    public static GeneratorDriver DriveSource(IIncrementalGenerator generator, IReadOnlyList<string> sources, bool verifyNoErrors = false, MetadataReference[]? additionalMetadata = null)
    {
        var syntaxTrees = sources.Select((x, i) => CSharpSyntaxTree.ParseText(x, path: $"source{i}.cs"));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Ww.Winter.Generator.Tests__",
            syntaxTrees: syntaxTrees,
            references: [
                MetadataReference.CreateFromFile(typeof(QueryAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IQueryable<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IListSource).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
                .. additionalMetadata ?? []
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );

        if (verifyNoErrors)
        {
            var errors = compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            errors.Should().BeEmpty();
        }

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return driver;
    }
}
