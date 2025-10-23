using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Ww.Winter.Generator.Model;

namespace Ww.Winter.Generator.QueryableFilters;

public sealed record QueryableFilterToGenerate(
    EntityModel Filter,
    EntityModel Entity
)
{
    public static QueryableFilterToGenerate Create(SemanticModel semanticModel, ClassDeclarationSyntax filterClass)
    {
        AttributeSyntax? attribute = null;

        foreach (var a in filterClass.AttributeLists.SelectMany(x => x.Attributes))
        {
            // do as much as possible syntax checks first for performance
            string name;
            var nameSyntax = a.Name;
            if (nameSyntax is QualifiedNameSyntax qualifiedName)
            {
                name = qualifiedName.Right.Identifier.ValueText;
            }
            else if (nameSyntax is IdentifierNameSyntax identifierName)
            {
                name = identifierName.Identifier.ValueText;
            }
            else
            {
                throw new InvalidOperationException($"INTERNAL ERROR: Unknown attribute name syntax: '{nameSyntax.GetType().Name}'.");
            }
            if (name != "QueryableFilter" && name != "QueryableFilterAttribute")
            {
                continue;
            }

            if (attribute is not null)
            {
                throw new InvalidOperationException("INTERNAL ERROR: Multiple QueryableFilter attributes found on the same entity.");
            }
            attribute = a;
        }
        if (attribute is null)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Missing QueryableFilter attribute.");
        }

        var expression = TypeModel.TryGetTypeOfExpression(attribute, 0)
            ?? throw new InvalidOperationException($"Cannot find n-th (argumentIndex) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax predefinedTypeOf)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Predefined type cannot be entity.");
        }

        var symbol = TypeModel.TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("INTERNAL ERROR: Missing named symbol.");

        return new QueryableFilterToGenerate(
            EntityModel.FromSymbol(symbol, 0),
            EntityModel.FromSyntax(semanticModel, filterClass, 0)
        );
    }
}
