using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ww.Winter.Generator.QueryFilters;

[Generator]
public sealed class QueryFilterIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<QueryFilterToGenerate> toGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(QueryFilterAttribute.FullTypeName,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (cxt, _) => GetQueryFilter(cxt)
            );

        context.RegisterSourceOutput(toGenerate, ExecuteQuery);
    }

    private static void ExecuteQuery(SourceProductionContext context, QueryFilterToGenerate toGenerate)
    {
        var (result, filename) = QueryFilterRenderer.Render(toGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }

    private static QueryFilterToGenerate GetQueryFilter(GeneratorAttributeSyntaxContext context)
    {
        var ownedBy = (ClassDeclarationSyntax)context.TargetNode;
        return QueryFilterToGenerate.Create(context.SemanticModel, ownedBy);
    }
}