using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Ww.Winter.Generator.Model;

public record PropertyModel(
    string Name,
    PropertyTypeModel Type
)
{
    public static PropertyModel FromSyntax(PropertyDeclarationSyntax syntax, SemanticModel semanticModel, int maxDepth)
    {
        var typeSyntax = syntax.Type;
        if (typeSyntax is null)
        {
            throw new InvalidOperationException($"Unable to get type from PropertyDeclarationSyntax. '{syntax}'");
        }
        return new PropertyModel(
            syntax.Identifier.ValueText,
            PropertyTypeModel.FromSyntax(semanticModel, typeSyntax, maxDepth)
        );
    }

    public static PropertyModel FromSymbol(IPropertySymbol symbol, int maxDepth)
    {
        return new PropertyModel(
            symbol.Name,
            PropertyTypeModel.FromSymbol(symbol.Type, maxDepth)
        );
    }
}
