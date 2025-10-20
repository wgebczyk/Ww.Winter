using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.QueryableFilters;

public sealed record QueryableFilterToGenerate
{
    public QueryableFilterEntity Filter { get; }
    public QueryableFilterEntity Entity { get; }

    public QueryableFilterToGenerate(
        QueryableFilterEntity filter,
        QueryableFilterEntity entity
    )
    {
        Filter = filter;
        Entity = entity;
    }

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

        return new QueryableFilterToGenerate(
            entity: QueryableFilterEntity.FromAttributeArgument(semanticModel, attribute, 0),
            filter: QueryableFilterEntity.FromClassDeclaration(semanticModel, filterClass)
        );
    }
}

public static class QueryableFilter
{
    public static INamedTypeSymbol GetSymbolFromTypeOf(SemanticModel semanticModel, TypeOfExpressionSyntax expression)
    {
        SymbolInfo symbolInfo;
        if (expression.Type is QualifiedNameSyntax qualifiedTypeOf)
        {
            symbolInfo = semanticModel.GetSymbolInfo(qualifiedTypeOf.Right);
        }
        else if (expression.Type is IdentifierNameSyntax identifierTypeOf)
        {
            symbolInfo = semanticModel.GetSymbolInfo(identifierTypeOf);
        }
        else
        {
            throw new InvalidOperationException("INTERNAL ERROR: Unsupported entity type syntax in 'typeof(...)' expression.");
        }

        if (symbolInfo.Symbol is not INamedTypeSymbol entitySymbol)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Missing entity symbol.");
        }
        return entitySymbol;
    }

    public static string GetTypeName(INamedTypeSymbol symbol)
    {
        static string? GetParentNamePath(INamedTypeSymbol value, int maxNestingLevel)
        {
            if (maxNestingLevel <= 0)
            {
                throw new InvalidOperationException("Too much! If rumors say '640K ought to be enough for anybody', then 2 levels of nesting is enough as well.");
            }
            if (value.ContainingType != null)
            {
                if (maxNestingLevel >= 1)
                {
                    var parentPath = GetParentNamePath(value.ContainingType, maxNestingLevel - 1);
                    if (parentPath != null)
                    {
                        return $"{parentPath}.{value.ContainingType.Name}";
                    }
                    return value.ContainingType.Name;
                }
            }
            return null;
        }
        var parentPath = GetParentNamePath(symbol, 2);
        if (parentPath != null)
        {
            return $"{parentPath}.{symbol.Name}";
        }
        return symbol.Name;
    }

    public static string GetNamespace(ITypeSymbol symbol)
    {
        return symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToString();
    }
    public static string GetFullyQualifiedTypeName(ITypeSymbol symbol)
    {
        return symbol.ToString();
    }
}

public record QueryableFilterType
{
    public string Namespace { get; }
    public string FullyQualifiedTypeName { get; }
    public string TypeName { get; }

    public QueryableFilterType(
        string @namespace,
        string fullyQualifiedTypeName,
        string typeName
    )
    {
        Namespace = @namespace;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        TypeName = typeName;
    }

    public static QueryableFilterType FromSyntax(TypeSyntax syntax)
    {
        return new QueryableFilterType(
            "", // TODO
            syntax.ToString(),
            syntax.ToString()
        );
    }
    public static QueryableFilterType FromSymbol(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            return FromSymbol(namedTypeSymbol);
        }
        return new QueryableFilterType(
            @namespace: QueryableFilter.GetNamespace(symbol),
            fullyQualifiedTypeName: QueryableFilter.GetFullyQualifiedTypeName(symbol),
            typeName: symbol.Name
        );
    }
    public static QueryableFilterType FromSymbol(INamedTypeSymbol symbol)
    {
        return new QueryableFilterType(
            @namespace: QueryableFilter.GetNamespace(symbol),
            fullyQualifiedTypeName: QueryableFilter.GetFullyQualifiedTypeName(symbol),
            typeName: QueryableFilter.GetTypeName(symbol)
        );
    }
    public static QueryableFilterType FromSymbol(ClassDeclarationSyntax syntax)
    {
        var namespaceSyntax = syntax.Parent as BaseNamespaceDeclarationSyntax;
        string @namespace;
        if (namespaceSyntax.Name is QualifiedNameSyntax qualifiedNamespace)
        {
            @namespace = qualifiedNamespace.Left.ToFullString() + "." + qualifiedNamespace.Right.Identifier.ValueText;
        }
        else if (namespaceSyntax.Name is IdentifierNameSyntax identifierNamespace)
        {
            @namespace = identifierNamespace.Identifier.ValueText;
        }
        else
        {
            throw new InvalidOperationException("INTERNAL ERROR: Unsupported entity type syntax in 'typeof(...)' expression.");
        }
        var name = syntax.Identifier.ValueText;

        return new QueryableFilterType(
            @namespace: @namespace,
            fullyQualifiedTypeName: $"{@namespace}.{name}",
            typeName: name
        );
    }
}

public sealed record QueryableFilterEntity : QueryableFilterType
{
    public ImmutableArray<QueryProperty> Properties { get; }

    public QueryableFilterEntity(
        string @namespace,
        string fullyQualifiedEntityName,
        string entityName,
        ImmutableArray<QueryProperty> properties
    ) : base(@namespace, fullyQualifiedEntityName, entityName)
    {
        Properties = properties;
    }

    public static QueryableFilterEntity FromAttributeArgument(
        SemanticModel semanticModel,
        AttributeSyntax attribute,
        int argumentIndex
    )
    {
        var argumentList = attribute.ArgumentList
            ?? throw new InvalidOperationException("INTERNAL ERROR: Cannot find attribute's argument list.");
        if (argumentList.Arguments.Count <= argumentIndex)
        {
            throw new InvalidOperationException($"INTERNAL ERROR: Cannot find attribute argument at index {argumentIndex}");
        }
        var argument = argumentList.Arguments[argumentIndex]
            ?? throw new InvalidOperationException($"INTERNAL ERROR: Cannot find attribute argument at index {argumentIndex}");

        var symbol = QueryableFilter.GetSymbolFromTypeOf(semanticModel, (TypeOfExpressionSyntax)argument.Expression);
        var properties = symbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Select(QueryProperty.FromSymbol)
            .Where(x => x.PropertyName != "EqualityContract")
            .ToImmutableArray();
        return new QueryableFilterEntity(
            QueryableFilter.GetNamespace(symbol),
            QueryableFilter.GetFullyQualifiedTypeName(symbol),
            QueryableFilter.GetTypeName(symbol),
            properties
        );
    }
    public static QueryableFilterEntity FromClassDeclaration(
        SemanticModel semanticModel,
        ClassDeclarationSyntax classDeclaration
    )
    {
        var properties = classDeclaration
            .Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(QueryProperty.FromSyntax)
            .Where(x => x.PropertyName != "EqualityContract")
            .ToImmutableArray();
        return new QueryableFilterEntity(
            "", // TODO: classDeclaration.Parent?.ToString() ?? "",
            classDeclaration.Identifier.ValueText, // TODO: $"{classDeclaration.Parent?.ToString() ?? ""}.{classDeclaration.Identifier.ValueText}",
            classDeclaration.Identifier.ValueText,
            properties
        );
    }
}

public sealed record QueryProperty
{
    public string PropertyName { get; }
    public QueryableFilterType PropertyType { get; }

    public QueryProperty(
        string propertyName,
        QueryableFilterType propertyType
    )
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public static QueryProperty FromSymbol(IPropertySymbol symbol)
    {
        return new QueryProperty(
            propertyName: symbol.Name,
            propertyType: QueryableFilterType.FromSymbol(symbol.Type)
        );
    }
    public static QueryProperty FromSyntax(PropertyDeclarationSyntax propertySyntax)
    {
        return new QueryProperty(
            propertyName: propertySyntax.Identifier.ValueText,
            propertyType: QueryableFilterType.FromSyntax(propertySyntax.Type)
        );
    }
}
