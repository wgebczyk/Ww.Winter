using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ww.Winter.Generator.Primitives;

public record PropertyTypeModel(
    string Name,
    bool IsNullable,
    EntityModel? Entity
)
{
    public bool IsSimple => Entity is null;

    public static PropertyTypeModel FromSyntax(SemanticModel semanticModel, TypeSyntax syntax, int maxDepth)
    {
        if (syntax is NullableTypeSyntax nullableType)
        {
            var element = FromSyntax(semanticModel, nullableType.ElementType, maxDepth);
            return element with { IsNullable = true };
        }
        if (syntax is PredefinedTypeSyntax predefinedType)
        {
            return FromSyntaxCore(predefinedType);
        }
        if (syntax is NameSyntax namedType)
        {
            return FromSyntaxCore(namedType, semanticModel, maxDepth);
        }
        throw new InvalidOperationException($"Unsupported syntax {syntax}");
    }

    public static PropertyTypeModel FromSymbol(ITypeSymbol symbol, int maxDepth)
    {
        if (TrySpecialType(symbol, out var result))
        {
            return result;
        }

        if (maxDepth == 0)
        {
            return new PropertyTypeModel(Name: symbol.Name, IsNullable: false, Entity: null);
        }

        var entity = EntityModel.FromSymbol(symbol, maxDepth - 1);
        return new PropertyTypeModel(Name: entity.Type.Name, IsNullable: false, Entity: entity);
    }

    private static PropertyTypeModel FromSyntaxCore(PredefinedTypeSyntax syntax)
    {
        return new PropertyTypeModel(Name: syntax.ToString(), IsNullable: false, Entity: null);
    }
    private static PropertyTypeModel FromSyntaxCore(NameSyntax syntax, SemanticModel semanticModel, int maxDepth)
    {
        var typeSymbolInfo = semanticModel.GetSymbolInfo(syntax);
        var typeSymbol = typeSymbolInfo.Symbol;
        if (typeSymbol is not ITypeSymbol type)
        {
            throw new InvalidOperationException($"Unable to get symbol information from {syntax}");
        }

        return FromSymbol(type, maxDepth);
    }
    private static bool TrySpecialType(ITypeSymbol type, [MaybeNullWhen(false)] out PropertyTypeModel model)
    {
        string? name = null;
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
                name = "string";
                break;
            case SpecialType.System_Char:
                name = "char";
                break;

            case SpecialType.System_DateTime:
                name = "DateTime";
                break;

            case SpecialType.System_Boolean:
                name = "bool";
                break;
            case SpecialType.System_Byte:
                name = "byte";
                break;
            case SpecialType.System_SByte:
                name = "sbyte";
                break;
            case SpecialType.System_Int16:
                name = "short";
                break;
            case SpecialType.System_UInt16:
                name = "ushort";
                break;
            case SpecialType.System_Int32:
                name = "int";
                break;
            case SpecialType.System_UInt32:
                name = "uint";
                break;
            case SpecialType.System_Int64:
                name = "long";
                break;
            case SpecialType.System_UInt64:
                name = "ulong";
                break;
            case SpecialType.System_Decimal:
                name = "decimal";
                break;
            case SpecialType.System_Single:
                name = "float";
                break;
            case SpecialType.System_Double:
                name = "double";
                break;
        }
        if (name is not null)
        {
            model = new PropertyTypeModel(Name: name, IsNullable: false, Entity: null);
            return true;
        }

        model = null;
        return false;
    }
}
