using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.Model;

public record EntityModel(
    TypeModel Type,
    ImmutableArray<PropertyModel> Properties
)
{
    public static EntityModel FromSyntax(BaseTypeDeclarationSyntax syntax, SemanticModel semanticModel, int maxDepth)
    {
        var type = TypeModel.FromSyntax(syntax);
        var properties = syntax.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(x => PropertyModel.FromSyntax(x, semanticModel, maxDepth)).ToImmutableArray();

        return new EntityModel(type, properties);
    }

    internal static EntityModel FromSymbol(ITypeSymbol symbol, int maxDepth)
    {
        var type = TypeModel.FromSymbol(symbol);
        var properties = symbol.GetMembers().OfType<IPropertySymbol>().Select(x => PropertyModel.FromSymbol(x, maxDepth)).ToImmutableArray();

        return new EntityModel(type, properties);
    }
}
