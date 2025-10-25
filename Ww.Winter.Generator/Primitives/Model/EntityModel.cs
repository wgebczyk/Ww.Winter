using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.Primitives;

public record EntityModel(
    TypeModel Type,
    ImmutableArray<PropertyModel> Properties,
    ImmutableHashSet<string> Methods
)
{
    public static EntityModel FromSyntax(SemanticModel semanticModel, BaseTypeDeclarationSyntax syntax, int maxDepth)
    {
        var type = TypeModel.FromSyntax(syntax);
        var properties = syntax.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(x => PropertyModel.FromSyntax(x, semanticModel, maxDepth)).ToImmutableArray();
        var methods = syntax.ChildNodes().OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ValueText).ToImmutableHashSet();

        return new EntityModel(type, properties, methods);
    }

    public static EntityModel FromSymbol(ITypeSymbol symbol, int maxDepth)
    {
        var type = TypeModel.FromSymbol(symbol);
        var properties = symbol.GetMembers().OfType<IPropertySymbol>().Select(x => PropertyModel.FromSymbol(x, maxDepth)).ToImmutableArray();
        var methods = symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Ordinary)
            .Where(x => !x.IsImplicitlyDeclared)
            .Select(x => x.Name)
            .ToImmutableHashSet();

        return new EntityModel(type, properties, methods);
    }
}
