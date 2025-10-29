using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public record BasicQuery(
    string MethodName,
    EntityModel Entity,
    string? UseBaseQueryExpression
)
{
    public static BasicQuery Create(SemanticModel semanticModel, AttributeSyntax attribute)
    {
        return new BasicQuery(
            GetMethodNameFromArguments(attribute),
            GetEntityModelFromAttribute(semanticModel, attribute),
            GetUseBaseQueryFromAttribute(attribute)
        );
    }

    private static string GetMethodNameFromArguments(AttributeSyntax attribute)
    {
        var arguments = attribute.ArgumentList?.Arguments.ToArray() ?? [];
        if (arguments.Length < 2)
        {
            throw new InvalidOperationException($"Expected at least 2 arguments in [BasicQuery(...)].");
        }
        var methodNameExpression = arguments[1].Expression;
        if (methodNameExpression is LiteralExpressionSyntax literalSyntax)
        {
            return literalSyntax.Token.ValueText;
        }
        throw new InvalidOperationException($"Expected 2nd argument to be LiteralExpressionSyntax in [BasicQuery(...)], but found {methodNameExpression}.");
    }
    private static EntityModel GetEntityModelFromAttribute(SemanticModel semanticModel, AttributeSyntax attribute)
    {
        var expression = TypeModel.TryGetTypeOfExpression(attribute, 0)
            ?? throw new InvalidOperationException($"Cannot find n-th (0) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax)
        {
            throw new InvalidOperationException("Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("Missing named symbol.");

        return EntityModel.FromSymbol(symbol, 0);
    }
    private static string? GetUseBaseQueryFromAttribute(AttributeSyntax attribute)
    {
        var argumentList = attribute.ArgumentList
            ?? throw new InvalidOperationException("Cannot find attribute's argument list.");

        var useBaseQueryArgument = argumentList.Arguments.SingleOrDefault(x => x.NameEquals?.Name.Identifier.ValueText == "UseBaseQueryExpression");
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

        throw new InvalidOperationException("Unsupported attribute value expression.");
    }
}
