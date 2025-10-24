using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ww.Winter.Generator.BasicQueries;

[Generator]
public class BasicQueryIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<BasicQueryToGenerate> basicQueriesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BasicQueryAttribute.FullTypeName,
                predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: (cxt, _) => BasicQueryToGenerate.Create(cxt.SemanticModel, (TypeDeclarationSyntax)cxt.TargetNode)
            );

        context.RegisterSourceOutput(basicQueriesToGenerate, ExecuteBasicQuery);
    }

    public static void ExecuteBasicQuery(SourceProductionContext context, BasicQueryToGenerate basicQueryToGenerate)
    {
        var (result, filename) = BasicQueryRenderer.Render(basicQueryToGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }
}