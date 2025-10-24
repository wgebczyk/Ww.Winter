using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public record BasicQuery(
    string MethodName,
    EntityModel Entity
)
{
    public static BasicQuery Create(SemanticModel semanticModel, AttributeSyntax attribute)
    {
        var methodName = GetMethodNameFromAttribute(attribute);

        var expression = TypeModel.TryGetTypeOfExpression(attribute, 0)
            ?? throw new InvalidOperationException($"Cannot find n-th (0) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("INTERNAL ERROR: Missing named symbol.");
        return new BasicQuery(
            methodName,
            EntityModel.FromSymbol(symbol, 0)
        );
    }

    private static string GetMethodNameFromAttribute(AttributeSyntax attribute)
    {
        // (arguments[1].Expression as LiteralExpressionSyntax)?.Token.ValueText!
        var arguments = attribute.ArgumentList?.Arguments.ToArray() ?? [];
        if (arguments.Length != 2)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: Expected exacly 2 arguments in [BasicQuery(...)].");
        }
        var methodNameExpression = arguments[1].Expression;
        if (methodNameExpression is LiteralExpressionSyntax literalSyntax)
        {
            return literalSyntax.Token.ValueText;
        }
        throw new InvalidOperationException($"Expected 2nd argument to be LiteralExpressionSyntax in [BasicQuery(...)], but found {methodNameExpression}.");
    }
}
