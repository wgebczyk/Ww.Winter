using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ww.Winter.Generator.Model;

namespace Ww.Winter.Generator.Tests.Model;

public sealed class TypeModelShould
{
    [Theory]
    [InlineData("", "SomeType", "SomeType", new string[0], "class SomeType {}")]
    [InlineData("SomeNamespace", "SomeType", "SomeNamespace.SomeType", new string[0], "namespace SomeNamespace; class SomeType {}")]
    [InlineData("SomeNamespace", "SomeType", "SomeNamespace.SomeType", new string[0], "namespace SomeNamespace { class SomeType {} }")]

    [InlineData("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", new string[0], "namespace SomeNamespace.Nested; class SomeType {}")]
    [InlineData("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", new string[0], "namespace SomeNamespace.Nested { class SomeType {} }")]
    [InlineData("SomeNamespace.Nested", "SomeType", "SomeNamespace.Nested.SomeType", new string[0], "namespace SomeNamespace { namespace Nested { class SomeType {} } }")]
    public void ExtractInfoFromClassDeclarationSyntax(
        string expectedNamespace,
        string expectedName,
        string expectedFullyQualifiedName,
        IReadOnlyList<string> expectedParentTypes,
        string source
    )
    {
        var root = CSharpSyntaxTree.ParseText(source);

        var typeModel = TypeModel.FromSyntax(root.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single());

        typeModel.Should().NotBeNull();
        typeModel.Namespace.Should().Be(expectedNamespace);
        typeModel.Name.Should().Be(expectedName);
        typeModel.FullyQualifiedName.Should().Be(expectedFullyQualifiedName);
        typeModel.ParentTypes.Should().BeEquivalentTo(expectedParentTypes);
    }

    [Theory]
    [InlineData("", "SomeRecord", "SomeRecord", new string[0], "record SomeRecord()")]
    [InlineData("SomeNamespace", "SomeRecord", "SomeNamespace.SomeRecord", new string[0], "namespace SomeNamespace; record SomeRecord()")]
    [InlineData("SomeNamespace", "SomeRecord", "SomeNamespace.SomeRecord", new string[0], "namespace SomeNamespace { record SomeRecord() }")]

    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.SomeRecord", new string[0], "namespace SomeNamespace.Nested; record SomeRecord()")]
    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.SomeRecord", new string[0], "namespace SomeNamespace.Nested { record SomeRecord() }")]
    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.SomeRecord", new string[0], "namespace SomeNamespace { namespace Nested { record SomeRecord() } }")]
    public void ExtractInfoFromRecordDeclarationSyntax(
        string expectedNamespace,
        string expectedName,
        string expectedFullyQualifiedName,
        IReadOnlyList<string> expectedParentTypes,
        string source
    )
    {
        var root = CSharpSyntaxTree.ParseText(source);

        var typeModel = TypeModel.FromSyntax(root.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>().Single());

        typeModel.Should().NotBeNull();
        typeModel.Namespace.Should().Be(expectedNamespace);
        typeModel.Name.Should().Be(expectedName);
        typeModel.FullyQualifiedName.Should().Be(expectedFullyQualifiedName);
        typeModel.ParentTypes.Should().BeEquivalentTo(expectedParentTypes);
    }

    [Theory]
    [InlineData("", "SomeRecord", "Outer.SomeRecord", new string[] { "Outer" }, "record Outer { record SomeRecord() }")]
    [InlineData("SomeNamespace", "SomeRecord", "SomeNamespace.Outer.SomeRecord", new string[] { "Outer" }, "namespace SomeNamespace; record Outer { record SomeRecord() }")]
    [InlineData("SomeNamespace", "SomeRecord", "SomeNamespace.Outer.SomeRecord", new string[] { "Outer" }, "namespace SomeNamespace { record Outer { record SomeRecord() } }")]

    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.Outer.SomeRecord", new string[] { "Outer" }, "namespace SomeNamespace.Nested; record Outer { record SomeRecord() }")]
    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.Outer.SomeRecord", new string[] { "Outer" }, "namespace SomeNamespace.Nested { record Outer { record SomeRecord() } }")]
    [InlineData("SomeNamespace.Nested", "SomeRecord", "SomeNamespace.Nested.Outer.SomeRecord", new string[] { "Outer" }, "namespace SomeNamespace { namespace Nested { record Outer { record SomeRecord() } } }")]
    public void ExtractInfoFromNestedTypes(
        string expectedNamespace,
        string expectedName,
        string expectedFullyQualifiedName,
        IReadOnlyList<string> expectedParentTypes,
        string source
    )
    {
        var root = CSharpSyntaxTree.ParseText(source);

        var typeModel = TypeModel.FromSyntax(root.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>().Single(x => x.Identifier.ValueText == expectedName));

        typeModel.Namespace.Should().Be(expectedNamespace);
        typeModel.Name.Should().Be(expectedName);
        typeModel.FullyQualifiedName.Should().Be(expectedFullyQualifiedName);
        typeModel.ParentTypes.Should().BeEquivalentTo(expectedParentTypes);
    }

    [Fact]
    public void ExtractInfoFromAttributeTypeOf()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System;

            namespace SomeNamespace;

            [AttributeUsage(AttributeTargets.Class)]
            public class SomeTypeOfAttribute: Attribute {
                public Type Type1 { get; }
                public Type Type2 { get; }
                public Type Type3 { get; }
                public SomeTypeOfAttribute(Type type1, Type type2, Type type3) {
                    Type1 = type1;
                    Type2 = type2;
                    Type3 = type3;
                }
            }

            [SomeTypeOf(typeof(int), typeof(String), typeof(SomeTypeOfAttribute))]
            public class SomeClass {}
            """
        );
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

        var diagnostics = compilation.GetDiagnostics();
        diagnostics.Should().BeEmpty();

        var sematicModel = compilation.GetSemanticModel(syntaxTree);

        var classSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "SomeClass");
        var attributeSyntax = classSyntax.AttributeLists.Single().Attributes.Single();

        var typeModel1 = TypeModel.FromAttributeArgument(sematicModel, attributeSyntax, 0);
        var typeModel2 = TypeModel.FromAttributeArgument(sematicModel, attributeSyntax, 1);
        var typeModel3 = TypeModel.FromAttributeArgument(sematicModel, attributeSyntax, 2);

        typeModel1.Should().BeEquivalentTo(new TypeModel("System", "Int32", "System.Int32", []));
        typeModel2.Should().BeEquivalentTo(new TypeModel("System", "String", "System.String", []));
        typeModel3.Should().BeEquivalentTo(new TypeModel("SomeNamespace", "SomeTypeOfAttribute", "SomeNamespace.SomeTypeOfAttribute", []));
    }
}
