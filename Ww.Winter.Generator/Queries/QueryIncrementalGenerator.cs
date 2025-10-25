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
                transform: (cxt, _) => Query.CreateQueryElements(cxt.SemanticModel, (MethodDeclarationSyntax)cxt.TargetNode)
            )
            .Collect()
            .SelectMany((x, _) =>
                x.GroupBy(y => y.Item1)
                .Select(y => new QueryToGenerate(y.Key, y.First().Item2, [.. y.Select(z => z.Item3)]))
            );

        context.RegisterSourceOutput(queriesToGenerate, ExecuteQuery);
    }

    private static void ExecuteQuery(SourceProductionContext context, QueryToGenerate queryToGenerate)
    {
        var (result, filename) = QueryRenderer.Render(queryToGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }
}