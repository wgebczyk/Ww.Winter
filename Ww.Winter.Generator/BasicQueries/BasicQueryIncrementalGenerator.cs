using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ww.Winter.Generator.BasicQueries;

[Generator]
public class BasicQueryIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<BasicQueryToGenerate> basicQueriesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                BasicQueryAttribute.FullTypeName,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: GetBasicQueryTypeToGenerate
            );

        context.RegisterSourceOutput(basicQueriesToGenerate, ExecuteBasicQuery);
    }

    public static void ExecuteBasicQuery(SourceProductionContext context, BasicQueryToGenerate basicQueryToGenerate)
    {
        var (result, filename) = BasicQueryRenderer.Generate(basicQueryToGenerate);
        context.AddSource(filename, SourceText.From(result, Encoding.UTF8));
    }

    public static BasicQueryToGenerate GetBasicQueryTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        var attributes = ((ClassDeclarationSyntax)context.TargetNode).AttributeLists.SelectMany(x => x.Attributes).ToArray();
        var queries = attributes.Select(x => BasicQuery.FromAttribute(x, context.SemanticModel.Compilation)).ToList();

        ct.ThrowIfCancellationRequested();

        return new BasicQueryToGenerate(
            @namespace: context.TargetSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : context.TargetSymbol.ContainingNamespace.ToString(),
            fullyQualifiedClassName: context.TargetSymbol.ToString(),
            className: context.TargetSymbol.Name,
            queries
        );
    }
}