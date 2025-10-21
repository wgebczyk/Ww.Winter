using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ww.Winter.Generator.Model;

namespace Ww.Winter.Generator.Tests.Model;

public sealed class EntityModelShould
{
    [Fact]
    public void ExtractInfoFromClassDeclarationSyntax()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            namespace SomeNamespace.Nested;

            public class OtherType {
                public required string OtherString { get; set; }
                public required int SomeInt { get; set; }
                public required bool SomeBool { get; set; }
            }

            public class SomeType {
                public required System.String SomeString { get; set; }
                public required int SomeInt { get; set; }
                public required bool SomeBool { get; set; }
                public required OtherType Other { get; set; }
            }
            """);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Ww.Winter.Generator.Tests.Model",
            syntaxTrees: [syntaxTree],
            references: [
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var sematicModel = compilation.GetSemanticModel(syntaxTree);

        var diagnostics = compilation.GetDiagnostics();
        diagnostics.Should().BeEmpty();

        var entityModel = EntityModel.FromSyntax(
            syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "SomeType"),
            sematicModel,
            1
        );

        entityModel.Should().NotBeNull();
        entityModel.Type.Should().BeEquivalentTo(new TypeModel("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", []));
    }
}
