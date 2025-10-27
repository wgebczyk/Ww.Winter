using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.QueryFilters;

public sealed record QueryFilter(
    EntityModel Entity,
    EntityModel Filter
)
{
    public static QueryFilter Create(SemanticModel semanticModel, AttributeSyntax attribute)
    {
        return new QueryFilter(
            FromAttributeArgument(semanticModel, attribute, 0),
            FromAttributeArgument(semanticModel, attribute, 1)
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
            throw new InvalidOperationException("Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("Missing named symbol.");
        return EntityModel.FromSymbol(symbol, 0);
    }
}
