using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace Ww.Winter.Generator.Queries;

[Generator]
public sealed class QueryIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<QueryToGenerate> queriesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(QueryAttribute.FullTypeName,
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (cxt, _) => GetQuery(cxt)
            )
            .Collect()
            .SelectMany((x, _) =>
                x.GroupBy(y => y.Item1.FullyQualifiedTypeName)
                .Select(y => QueryToGenerate.Create(y.First().Item1, y.Select(x => x.Item2).ToArray()))
            );

        context.RegisterSourceOutput(queriesToGenerate, ExecuteQuery);
    }

    private static void ExecuteQuery(SourceProductionContext context, QueryToGenerate queryToGenerate)
    {
        var (result, filename) = QueryRenderer.Render(queryToGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }

    private static (QueryEntity, Query) GetQuery(GeneratorAttributeSyntaxContext context)
    {
        var method = (MethodDeclarationSyntax)context.TargetNode;
        var ownedBy = (ClassDeclarationSyntax)method.Parent!;
        var ownedByType = Query.GetSymbol(context.SemanticModel.Compilation, Query.GetName(ownedBy));
        var attribute = method.AttributeLists.SelectMany(x => x.Attributes).Single();
        return (QueryEntity.FromSymbol(ownedByType), Query.Create(context.SemanticModel.Compilation, attribute, method));
    }
}