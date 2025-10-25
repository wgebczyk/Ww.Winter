using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Tests.Primitives.Model;

public sealed class EntityModelShould
{
    [Fact]
    public void ExtractInfoFromClassDeclarationSyntax()
    {
        var source =
            """
            namespace SomeNamespace.Nested;

            public class IgnoredType {
                public required string IgnoredString { get; set; }
            }

            public class ForeignType {
                public required string ForeignString { get; set; }
                public required int ForeignInt { get; set; }
                public required bool ForeignBool { get; set; }
                public required IgnoredType Ignored { get; set; }
            }

            public class OtherType {
                public required string OtherString { get; set; }
                public required int OtherInt { get; set; }
                public required bool OtherBool { get; set; }
                public required ForeignType Foreign { get; set; }
            }

            public class SomeType {
                public required System.String SomeString { get; set; }
                public required int SomeInt { get; set; }
                public required bool SomeBool { get; set; }
                public required OtherType Other { get; set; }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

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
            sematicModel,
            syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "SomeType"),
            2
        );

        entityModel.Should().BeEquivalentTo(new EntityModel(
            new TypeModel("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", false, []),
            [
                new PropertyModel("SomeString", new PropertyTypeModel("string", false, null)),
                new PropertyModel("SomeInt", new PropertyTypeModel("int", false, null)),
                new PropertyModel("SomeBool", new PropertyTypeModel("bool", false, null)),
                new PropertyModel("Other", new PropertyTypeModel("OtherType", false, new EntityModel(
                    new TypeModel("SomeNamespace.Nested", "OtherType", "SomeNamespace.Nested.OtherType", false, []),
                    [
                        new PropertyModel("OtherString", new PropertyTypeModel("string", false, null)),
                        new PropertyModel("OtherInt", new PropertyTypeModel("int", false, null)),
                        new PropertyModel("OtherBool", new PropertyTypeModel("bool", false, null)),
                        new PropertyModel("Foreign", new PropertyTypeModel("ForeignType", false, new EntityModel(
                            new TypeModel("SomeNamespace.Nested", "ForeignType", "SomeNamespace.Nested.ForeignType", false, []),
                            [
                                new PropertyModel("ForeignString", new PropertyTypeModel("string", false, null)),
                                new PropertyModel("ForeignInt", new PropertyTypeModel("int", false, null)),
                                new PropertyModel("ForeignBool", new PropertyTypeModel("bool", false, null)),
                                new PropertyModel("Ignored", new PropertyTypeModel("IgnoredType", false, null)),
                            ], []
                        )))
                    ], []
                )))
            ], []
        ));
    }
    [Fact]
    public void ExtractInfoFromNestedClassDeclarationSyntax()
    {
        var source =
            """
            namespace SomeNamespace {
                namespace Nested {

                    public class SomeType {
                        public int SomeInt { get; set; }
                        public OtherType Other { get; set; }

                        public class OtherType {
                            public int OtherInt { get; set; }
                            public ForeignType Foreign { get; set; }

                            public class ForeignType {
                                public int ForeignInt { get; set; }
                                public IgnoredType Ignored { get; set; }

                                public class IgnoredType {
                                    public int IgnoredInt { get; set; }
                                }
                            }
                        }
                    }

                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

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
            sematicModel,
            syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "SomeType"),
            2
        );

        entityModel.Should().BeEquivalentTo(new EntityModel(
            new TypeModel("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", false, []),
            [
                new PropertyModel("SomeInt", new PropertyTypeModel("int", false, null)),
                new PropertyModel("Other", new PropertyTypeModel("SomeType.OtherType", false, new EntityModel(
                    new TypeModel("SomeNamespace.Nested", "SomeType.OtherType", "SomeNamespace.Nested.SomeType.OtherType", false, [new TypeNameModel("SomeType", false)]),
                    [
                        new PropertyModel("OtherInt", new PropertyTypeModel("int", false, null)),
                        new PropertyModel("Foreign", new PropertyTypeModel("SomeType.OtherType.ForeignType", false, new EntityModel(
                            new TypeModel("SomeNamespace.Nested", "SomeType.OtherType.ForeignType", "SomeNamespace.Nested.SomeType.OtherType.ForeignType", false, [new TypeNameModel("SomeType", false), new TypeNameModel("OtherType", false)]),
                            [
                                new PropertyModel("ForeignInt", new PropertyTypeModel("int", false, null)),
                                new PropertyModel("Ignored", new PropertyTypeModel("IgnoredType", false, null)),
                            ], []
                        )))
                    ], []
                )))
            ], []
        ));
    }
}
