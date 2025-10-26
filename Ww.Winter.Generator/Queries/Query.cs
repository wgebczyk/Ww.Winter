using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Queries;

public sealed record Query(
    EntityModel Entity,
    EntityModel Filter,
    string MethodName,
    string FilterParamName,
    string? SortParamName,
    string? PaginationParamName,

    string? UseBaseQuery
)
{
    public static (TypeModel, ImmutableHashSet<MethodModel>, Query) CreateQueryElements(SemanticModel semanticModel, MethodDeclarationSyntax syntax)
    {
        var parentSyntax = syntax.Parent;
        if (parentSyntax is not TypeDeclarationSyntax typeDeclarationSyntax)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: [Query(...)] attributed method does not belong to type.");
        }

        var ownedByType = TypeModel.FromSyntax(typeDeclarationSyntax);
        var ownedByMethods = parentSyntax.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(x => !x.Modifiers.Any(y => y.IsKind(SyntaxKind.PartialKeyword)))
            .Select(x => MethodModel.FromSyntax(semanticModel, x))
            .ToImmutableHashSet();
        var query = Create(semanticModel, syntax);
        return (ownedByType, ownedByMethods, query);
    }

    public static Query Create(SemanticModel semanticModel, MethodDeclarationSyntax syntax)
    {
        var attribute = syntax.AttributeLists.SelectMany(x => x.Attributes).Single();

        var parameters = syntax.ParameterList.Parameters;

        var filterParameter = parameters.Single(x => x.Identifier.ValueText.Contains("filter", StringComparison.OrdinalIgnoreCase));
        var sortParameter = parameters.SingleOrDefault(x => x.Identifier.ValueText.Contains("sort", StringComparison.OrdinalIgnoreCase));
        var paginationParameter = parameters.SingleOrDefault(x => x.Identifier.ValueText.Contains("pagination", StringComparison.OrdinalIgnoreCase));

        if (filterParameter.Type is null)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: Missing named symbol from filter parameter. {filterParameter}.");
        }

        var filterSymbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, filterParameter.Type)
            ?? throw new InvalidOperationException("INTERNAL ERROR: Missing named symbol for parameter type. {filterParameter.Type}");

        var methodName = syntax.Identifier.ValueText;

        return new Query(
            Entity: FromAttributeArgument(semanticModel, attribute, 0),
            Filter: EntityModel.FromSymbol(filterSymbol, 1),
            MethodName: syntax.Identifier.ValueText,
            filterParameter.Identifier.ValueText,
            sortParameter.Identifier.ValueText,
            paginationParameter.Identifier.ValueText,
            GetUseBaseQueryFromAttribute(attribute)
        );
    }

    private static EntityModel FromAttributeArgument(
        SemanticModel semanticModel,
        AttributeSyntax attribute,
        int argumentIndex
    )
    {
        var expression = TypeModel.TryGetTypeOfExpression(attribute, argumentIndex)
            ?? throw new InvalidOperationException($"Cannot find n-th ({argumentIndex}) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("INTERNAL ERROR: Missing named symbol.");
        return EntityModel.FromSymbol(symbol, 0);
    }

    private static string? GetUseBaseQueryFromAttribute(AttributeSyntax attribute)
    {
        var argumentList = attribute.ArgumentList
            ?? throw new InvalidOperationException("INTERNAL ERROR: Cannot find attribute's argument list.");

        var useBaseQueryArgument = argumentList.Arguments.SingleOrDefault(x => x.NameEquals?.Name.Identifier.ValueText == "UseBaseQuery");
        if (useBaseQueryArgument is null)
        {
            return null;
        }
        if (useBaseQueryArgument.Expression is LiteralExpressionSyntax literalExpression)
        {
            return literalExpression.Token.ValueText;
        }
        if (useBaseQueryArgument.Expression is InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression is IdentifierNameSyntax nameSyntax && nameSyntax.Identifier.ValueText == "nameof")
            {
                var firstArgument = invocationExpression.ArgumentList.Arguments.Single();
                if (firstArgument.Expression is IdentifierNameSyntax argumentIdentifierExpression)
                {
                    return argumentIdentifierExpression.Identifier.ValueText;
                }
            }
        }

        throw new InvalidOperationException("INTERNAL ERROR: Unsupported attribute value expression.");
    }
}
