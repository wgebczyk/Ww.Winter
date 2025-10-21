using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ww.Winter.Generator.QueryFilters;

public sealed record QueryFilterToGenerate
{
    public QueryFilterType OwnedBy { get; }
    public ImmutableArray<QueryFilter> QueryFilters { get; }

    public QueryFilterToGenerate(
        QueryFilterType ownedBy,
        ImmutableArray<QueryFilter> queryFilters
    )
    {
        OwnedBy = ownedBy;
        QueryFilters = queryFilters;
    }

    public static QueryFilterToGenerate Create(SemanticModel semanticModel, ClassDeclarationSyntax ownedBy)
    {
        var queryFilters = new List<QueryFilter>();
        foreach (var attribute in ownedBy.AttributeLists.SelectMany(x => x.Attributes))
        {
            // do as much as possible syntax checks first for performance
            string name;
            var nameSyntax = attribute.Name;
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
            if (name != "QueryFilter" && name != "QueryFilterAttribute")
            {
                continue;
            }

            queryFilters.Add(QueryFilter.Create(semanticModel, attribute));
        }
        return new QueryFilterToGenerate(
            ownedBy: QueryFilterType.FromSymbol(ownedBy),
            queryFilters: queryFilters.ToImmutableArray()
        );
    }
}

public sealed record QueryFilter
{
    public QueryFilterEntity Entity { get; }
    public QueryFilterEntity Filter { get; }

    public QueryFilter(
        QueryFilterEntity entity,
        QueryFilterEntity filter
    )
    {
        Entity = entity;
        Filter = filter;
    }

    public static QueryFilter Create(SemanticModel semanticModel, AttributeSyntax attribute)
    {
        return new QueryFilter(
            QueryFilterEntity.FromAttributeArgument(semanticModel, attribute, 0),
            QueryFilterEntity.FromAttributeArgument(semanticModel, attribute, 1)
        );
    }

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

public record QueryFilterType
{
    public string Namespace { get; }
    public string FullyQualifiedTypeName { get; }
    public string TypeName { get; }

    public QueryFilterType(
        string @namespace,
        string fullyQualifiedTypeName,
        string typeName
    )
    {
        Namespace = @namespace;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        TypeName = typeName;
    }

    public static QueryFilterType FromSymbol(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            return FromSymbol(namedTypeSymbol);
        }
        return new QueryFilterType(
            @namespace: QueryFilter.GetNamespace(symbol),
            fullyQualifiedTypeName: QueryFilter.GetFullyQualifiedTypeName(symbol),
            typeName: symbol.Name
        );
    }
    public static QueryFilterType FromSymbol(INamedTypeSymbol symbol)
    {
        return new QueryFilterType(
            @namespace: QueryFilter.GetNamespace(symbol),
            fullyQualifiedTypeName: QueryFilter.GetFullyQualifiedTypeName(symbol),
            typeName: QueryFilter.GetTypeName(symbol)
        );
    }
    public static QueryFilterType FromSymbol(ClassDeclarationSyntax syntax)
    {
        var namespaceSyntax = syntax.Parent as BaseNamespaceDeclarationSyntax;
        string @namespace;
        if (namespaceSyntax?.Name is QualifiedNameSyntax qualifiedNamespace)
        {
            @namespace = qualifiedNamespace.Left.ToFullString() + "." + qualifiedNamespace.Right.Identifier.ValueText;
        }
        else if (namespaceSyntax?.Name is IdentifierNameSyntax identifierNamespace)
        {
            @namespace = identifierNamespace.Identifier.ValueText;
        }
        else
        {
            throw new InvalidOperationException("INTERNAL ERROR: Unsupported entity type syntax in 'typeof(...)' expression.");
        }
        var name = syntax.Identifier.ValueText;

        return new QueryFilterType(
            @namespace: @namespace,
            fullyQualifiedTypeName: $"{@namespace}.{name}",
            typeName: name
        );
    }
}

public sealed record QueryFilterEntity : QueryFilterType
{
    public ImmutableArray<QueryProperty> Properties { get; }

    public QueryFilterEntity(
        string @namespace,
        string fullyQualifiedEntityName,
        string entityName,
        ImmutableArray<QueryProperty> properties
    ) : base(@namespace, fullyQualifiedEntityName, entityName)
    {
        Properties = properties;
    }

    public static QueryFilterEntity FromAttributeArgument(
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

        var symbol = QueryFilter.GetSymbolFromTypeOf(semanticModel, (TypeOfExpressionSyntax)argument.Expression);
        var properties = symbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Select(QueryProperty.FromSymbol)
            .Where(x => x.PropertyName != "EqualityContract")
            .ToImmutableArray();
        return new QueryFilterEntity(
            QueryFilter.GetNamespace(symbol),
            QueryFilter.GetFullyQualifiedTypeName(symbol),
            QueryFilter.GetTypeName(symbol),
            properties
        );
    }
}

public sealed record QueryProperty
{
    public string PropertyName { get; }
    public QueryFilterType PropertyType { get; }

    public QueryProperty(
        string propertyName,
        QueryFilterType propertyType
    )
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public static QueryProperty FromSymbol(IPropertySymbol symbol)
    {
        return new QueryProperty(
            propertyName: symbol.Name,
            propertyType: QueryFilterType.FromSymbol(symbol.Type)
        );
    }
}
