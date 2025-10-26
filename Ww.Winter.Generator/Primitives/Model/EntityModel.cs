using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.Primitives;

public record MethodModel(
    string Name,
    TypeModel Type
)
{
    public static MethodModel FromSyntax(SemanticModel semanticModel, MethodDeclarationSyntax syntax)
    {
        var returnType = TypeModel.FromSyntax(syntax.ReturnType);

        return new MethodModel(syntax.Identifier.ValueText, returnType);
    }
    public static MethodModel FromSymbol(IMethodSymbol symbol)
    {
        var returnType = TypeModel.FromSymbol(symbol.ReturnType);

        return new MethodModel(symbol.Name, returnType);
    }
}

public record EntityModel(
    TypeModel Type,
    ImmutableArray<PropertyModel> Properties,
    ImmutableHashSet<MethodModel> Methods
)
{
    public static EntityModel FromSyntax(SemanticModel semanticModel, BaseTypeDeclarationSyntax syntax, int maxDepth)
    {
        var type = TypeModel.FromSyntax(syntax);
        var properties = syntax.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(x => PropertyModel.FromSyntax(x, semanticModel, maxDepth)).ToImmutableArray();
        var methods = syntax.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(x => !x.Modifiers.Any(y => y.IsKind(SyntaxKind.PartialKeyword)))
            .Select(x => MethodModel.FromSyntax(semanticModel, x))
            .ToImmutableHashSet();

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
            .Select(MethodModel.FromSymbol)
            .ToImmutableHashSet();

        return new EntityModel(type, properties, methods);
    }
}
