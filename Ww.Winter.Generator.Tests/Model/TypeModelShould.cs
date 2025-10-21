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
}
