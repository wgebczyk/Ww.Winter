using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Tests.Primitives.Model;

public sealed class PropertyTypeModelShould
{
    [Fact]
    public void ExtractInfoFromKeywordTypes()
    {
        var source =
            """
            namespace SomeNamespace;

            public class SomeType {
                public string SomeString { get; set; }
                public char SomeChar { get; set; }
                public bool SomeBoolean { get; set; }
                public byte SomeByte { get; set; }
                public sbyte SomeSByte { get; set; }
                public short SomeInt16 { get; set; }
                public ushort SomeUInt16 { get; set; }
                public int SomeInt32 { get; set; }
                public uint SomeUInt32 { get; set; }
                public long SomeInt64 { get; set; }
                public ulong SomeUInt64 { get; set; }
                public decimal SomeDecimal { get; set; }
                public float SomeSingle { get; set; }
                public double SomeDouble { get; set; }
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
            1
        );

        entityModel.Should().BeEquivalentTo(new EntityModel(
            new TypeModel("SomeNamespace", "SomeType", "SomeNamespace.SomeType", false, []),
            [
                new PropertyModel("SomeString", new PropertyTypeModel("string", false, null)),
                new PropertyModel("SomeChar", new PropertyTypeModel("char", false, null)),
                new PropertyModel("SomeBoolean", new PropertyTypeModel("bool", false, null)),
                new PropertyModel("SomeByte", new PropertyTypeModel("byte", false, null)),
                new PropertyModel("SomeSByte", new PropertyTypeModel("sbyte", false, null)),
                new PropertyModel("SomeInt16", new PropertyTypeModel("short", false, null)),
                new PropertyModel("SomeUInt16", new PropertyTypeModel("ushort", false, null)),
                new PropertyModel("SomeInt32", new PropertyTypeModel("int", false, null)),
                new PropertyModel("SomeUInt32", new PropertyTypeModel("uint", false, null)),
                new PropertyModel("SomeInt64", new PropertyTypeModel("long", false, null)),
                new PropertyModel("SomeUInt64", new PropertyTypeModel("ulong", false, null)),
                new PropertyModel("SomeDecimal", new PropertyTypeModel("decimal", false, null)),
                new PropertyModel("SomeSingle", new PropertyTypeModel("float", false, null)),
                new PropertyModel("SomeDouble", new PropertyTypeModel("double", false, null)),
            ], []
        ));
    }
    [Fact]
    public void ExtractInfoFromSystemTypes()
    {
        var source =
            """
            using System;

            namespace SomeNamespace;

            public class SomeType {
                public String SomeString { get; set; }
                public Char SomeChar { get; set; }
                public DateTime SomeDateTime { get; set; }
                public Boolean SomeBoolean { get; set; }
                public Byte SomeByte { get; set; }
                public SByte SomeSByte { get; set; }
                public Int16 SomeInt16 { get; set; }
                public UInt16 SomeUInt16 { get; set; }
                public Int32 SomeInt32 { get; set; }
                public UInt32 SomeUInt32 { get; set; }
                public Int64 SomeInt64 { get; set; }
                public UInt64 SomeUInt64 { get; set; }
                public Decimal SomeDecimal { get; set; }
                public Single SomeSingle { get; set; }
                public Double SomeDouble { get; set; }
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
            1
        );

        entityModel.Should().BeEquivalentTo(new EntityModel(
            new TypeModel("SomeNamespace", "SomeType", "SomeNamespace.SomeType", false, []),
            [
                new PropertyModel("SomeString", new PropertyTypeModel("string", false, null)),
                new PropertyModel("SomeChar", new PropertyTypeModel("char", false, null)),
                new PropertyModel("SomeDateTime", new PropertyTypeModel("DateTime", false, null)),
                new PropertyModel("SomeBoolean", new PropertyTypeModel("bool", false, null)),
                new PropertyModel("SomeByte", new PropertyTypeModel("byte", false, null)),
                new PropertyModel("SomeSByte", new PropertyTypeModel("sbyte", false, null)),
                new PropertyModel("SomeInt16", new PropertyTypeModel("short", false, null)),
                new PropertyModel("SomeUInt16", new PropertyTypeModel("ushort", false, null)),
                new PropertyModel("SomeInt32", new PropertyTypeModel("int", false, null)),
                new PropertyModel("SomeUInt32", new PropertyTypeModel("uint", false, null)),
                new PropertyModel("SomeInt64", new PropertyTypeModel("long", false, null)),
                new PropertyModel("SomeUInt64", new PropertyTypeModel("ulong", false, null)),
                new PropertyModel("SomeDecimal", new PropertyTypeModel("decimal", false, null)),
                new PropertyModel("SomeSingle", new PropertyTypeModel("float", false, null)),
                new PropertyModel("SomeDouble", new PropertyTypeModel("double", false, null)),
            ], []
        ));
    }
}
