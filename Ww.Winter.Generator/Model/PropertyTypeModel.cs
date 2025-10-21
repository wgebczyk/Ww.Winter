using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ww.Winter.Generator.Model;

public record PropertyTypeModel(
    string Name,
    EntityModel? Entity
)
{
    public bool IsSimple => Entity is null;

    public static PropertyTypeModel FromSyntax(TypeSyntax syntax, SemanticModel semanticModel, int maxDepth)
    {
        if (syntax is PredefinedTypeSyntax predefinedType)
        {
            return FromSyntaxCore(predefinedType);
        }
        if (syntax is NameSyntax namedType)
        {
            return FromSyntaxCore(namedType, semanticModel, maxDepth);
        }
        return new PropertyTypeModel(Name: "TODO", Entity: null);
    }

    public static PropertyTypeModel FromSymbol(ITypeSymbol symbol, int maxDepth)
    {
        if (TrySpecialType(symbol, out var result))
        {
            return result;
        }

        if (maxDepth == 0)
        {
            return new PropertyTypeModel(Name: symbol.Name, Entity: null);
        }
        return new PropertyTypeModel(Name: symbol.Name, Entity: EntityModel.FromSymbol(symbol, maxDepth - 1));
    }

    private static PropertyTypeModel FromSyntaxCore(PredefinedTypeSyntax syntax)
    {
        return new PropertyTypeModel(Name: syntax.ToString(), Entity: null);
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
        switch (type.SpecialType)
        {
            case SpecialType.System_String:
            case SpecialType.System_Char:

            case SpecialType.System_DateTime:
            case SpecialType.System_Enum:

            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Decimal:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
                model = new PropertyTypeModel(Name: type.Name, Entity: null);
                return true;
        }
        model = null;
        return false;
    }
}
