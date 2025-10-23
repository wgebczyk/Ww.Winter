using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ww.Winter.Generator.QueryableFilters;

[Generator]
public sealed class QueryableFilterIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<QueryableFilterToGenerate> toGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(QueryableFilterAttribute.FullTypeName,
                predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: (cxt, _) => QueryableFilterToGenerate.Create(cxt.SemanticModel, (TypeDeclarationSyntax)cxt.TargetNode)
            );

        context.RegisterSourceOutput(toGenerate, ExecuteQuery);
    }

    private static void ExecuteQuery(SourceProductionContext context, QueryableFilterToGenerate toGenerate)
    {
        var (result, filename) = QueryableFilterRenderer.Render(toGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }
}