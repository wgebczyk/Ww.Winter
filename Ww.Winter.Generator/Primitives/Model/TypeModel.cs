using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.Primitives;

public record TypeModel(
    string Namespace,
    string Name,
    string FullyQualifiedName,
    bool IsRecord,
    ImmutableArray<TypeNameModel> ParentTypes
)
{
    public static TypeModel FromSyntax(PredefinedTypeSyntax syntax)
    {
        string? name = null;
        switch (syntax.Keyword.Kind())
        {
            case SyntaxKind.StringKeyword:
                name = "String";
                break;
            case SyntaxKind.CharKeyword:
                name = "Char";
                break;

            case SyntaxKind.BoolKeyword:
                name = "Boolean";
                break;
            case SyntaxKind.ByteKeyword:
                name = "Byte";
                break;
            case SyntaxKind.SByteKeyword:
                name = "SByte";
                break;
            case SyntaxKind.ShortKeyword:
                name = "int16";
                break;
            case SyntaxKind.UShortKeyword:
                name = "UInt16";
                break;
            case SyntaxKind.IntKeyword:
                name = "Int32";
                break;
            case SyntaxKind.UIntKeyword:
                name = "UInt32";
                break;
            case SyntaxKind.LongKeyword:
                name = "Int64";
                break;
            case SyntaxKind.ULongKeyword:
                name = "UInt64";
                break;
            case SyntaxKind.DecimalKeyword:
                name = "Decimal";
                break;
            case SyntaxKind.FloatKeyword:
                name = "Single";
                break;
            case SyntaxKind.DoubleKeyword:
                name = "Double";
                break;
        }
        if (name is null)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: Unsupported PredefinedTypeSyntax {syntax}");
        }

        return new TypeModel("System", name, "System." + name, false, []);
    }
    public static TypeModel FromSyntax(TypeSyntax syntax)
    {
        if (syntax is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return FromSyntax(predefinedTypeSyntax);
        }
        return new TypeModel("", syntax.ToString(), syntax.ToString(), false, []);
    }

    public static TypeModel FromSyntax(BaseTypeDeclarationSyntax syntax)
    {
        var parentTypes = new List<TypeNameModel>(1);
        var parentNamespaces = new List<string>(2);

        SyntaxNode? node = syntax.Parent;
        while (node is not null)
        {
            if (node is BaseTypeDeclarationSyntax cSyntax)
            {
                var identifier = cSyntax.Identifier.ToString();
                parentTypes.Add(new TypeNameModel(identifier, cSyntax is RecordDeclarationSyntax));
            }
            else if (node is BaseNamespaceDeclarationSyntax nsSyntax)
            {
                var identifier = nsSyntax.Name.ToString();
                parentNamespaces.Add(identifier);
            }
            node = node.Parent;
        }
        parentTypes.Reverse();
        parentNamespaces.Reverse();

        var name = syntax.Identifier.ToString();
        var ns = string.Join(".", parentNamespaces);
        string[] allParts = [.. parentNamespaces, .. parentTypes.Select(x => x.Name), name];
        var fullyQualifiedName = string.Join(".", allParts);
        var isRecord = syntax is RecordDeclarationSyntax;
        return new TypeModel(ns, name, fullyQualifiedName, isRecord,[.. parentTypes]);
    }

    public static TypeModel FromSymbol(ITypeSymbol symbol)
    {
        var parentTypes = new List<TypeNameModel>(1);

        INamedTypeSymbol? node = symbol.ContainingType;
        while (node is not null)
        {
            parentTypes.Add(new TypeNameModel(node.Name, node.TypeKind == TypeKind.Class && node.IsRecord));
            node = node.ContainingType;
        }
        parentTypes.Reverse();

        var name = string.Join(".", [..parentTypes.Select(x => x.Name), symbol.Name]);
        var ns = symbol.ContainingNamespace.ToDisplayString();
        string[] allParts = [ns, name];
        var fullyQualifiedName = string.Join(".", allParts);
        var isRecord = symbol.TypeKind == TypeKind.Class && symbol.IsRecord;
        return new TypeModel(ns, name, fullyQualifiedName, isRecord, [.. parentTypes]);
    }

    public static TypeModel FromAttributeArgument(
        SemanticModel semanticModel,
        AttributeSyntax attribute,
        int argumentIndex
    )
    {
        var expression = TryGetTypeOfExpression(attribute, argumentIndex)
            ?? throw new InvalidOperationException($"Cannot find n-th (argumentIndex) typeof(...) argument.");
        if (expression.Type is PredefinedTypeSyntax predefinedTypeOf)
        {
            return FromSyntax(predefinedTypeOf);
        }

        var symbol = TryGetNamedTypeSymbol(semanticModel, expression.Type)
            ?? throw new InvalidOperationException("INTERNAL ERROR: Missing named symbol.");

        return FromSymbol(symbol);
    }

    public static TypeOfExpressionSyntax? TryGetTypeOfExpression(AttributeSyntax attribute, int argumentIndex)
    {
        var argumentList = attribute.ArgumentList
            ?? throw new InvalidOperationException("INTERNAL ERROR: Cannot find attribute's argument list.");
        if (argumentList.Arguments.Count <= argumentIndex)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: Cannot find attribute argument at index {argumentIndex}");
        }
        var argument = argumentList.Arguments[argumentIndex]
            ?? throw new InvalidOperationException($"INTERNAL ERROR: Cannot find attribute argument at index {argumentIndex}");

        return argument.Expression as TypeOfExpressionSyntax;
    }

    public static INamedTypeSymbol? TryGetNamedTypeSymbol(SemanticModel semanticModel, TypeSyntax syntax)
    {
        SymbolInfo symbolInfo;
        if (syntax is QualifiedNameSyntax qualifiedTypeOf)
        {
            symbolInfo = semanticModel.GetSymbolInfo(qualifiedTypeOf.Right);
        }
        else if (syntax is IdentifierNameSyntax identifierTypeOf)
        {
            symbolInfo = semanticModel.GetSymbolInfo(identifierTypeOf);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported type syntax {syntax}.");
        }

        return symbolInfo.Symbol as INamedTypeSymbol;
    }
}
