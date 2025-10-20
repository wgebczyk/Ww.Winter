using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ww.Winter.Generator.Queries;

public sealed record QueryToGenerate
{
    public QueryEntity OwnedBy { get; }
    public IReadOnlyList<Query> Queries { get; }

    public QueryToGenerate(
        QueryEntity ownedBy,
        IReadOnlyList<Query> queries
    )
    {
        OwnedBy = ownedBy;
        Queries = queries;
    }

    public static QueryToGenerate Create(QueryEntity ownedBy, Query[] queries)
    {
        return new QueryToGenerate(ownedBy, queries);
    }
}

public sealed record Query
{
    public QueryEntity Entity { get; }
    public QueryEntity FilterType { get; }
    public string MethodName { get; }
    public string FilterParamName { get; }
    public string SortParamName { get; }
    public string PaginationParamName { get; }

    public Query(
        QueryEntity entity,
        QueryEntity filterType,
        string methodName,
        string filterParamName,
        string sortParamName,
        string paginationParamName
    )
    {
        Entity = entity;
        FilterType = filterType;
        MethodName = methodName;
        FilterParamName = filterParamName;
        SortParamName = sortParamName;
        PaginationParamName = paginationParamName;
    }

    public static Query Create(Compilation compilation, AttributeSyntax attribute, MethodDeclarationSyntax methodSyntax)
    {
        var parameters = methodSyntax.ParameterList.Parameters.Select(GetName).ToList();

        var filterParamName = parameters.SingleOrDefault(x => x.Contains("filter", StringComparison.OrdinalIgnoreCase));
        var sortParamName = parameters.SingleOrDefault(x => x.Contains("sort", StringComparison.OrdinalIgnoreCase));
        var paginationParamName = parameters.SingleOrDefault(x => x.Contains("pagination", StringComparison.OrdinalIgnoreCase));

        var filterParameter = methodSyntax.ParameterList.Parameters.Single(x => x.Identifier.ValueText == filterParamName);
        var filterType = GetSymbol(compilation, GetName(filterParameter.Type!)!);
        var methodName = methodSyntax.Identifier.ValueText;

        return new Query(
            entity: QueryEntity.FromAttribute(attribute, compilation),
            filterType: QueryEntity.FromSymbol(filterType),
            methodName,
            filterParamName,
            sortParamName,
            paginationParamName
        );
    }

    public static ISymbol GetSymbol(Compilation compilation, string symbolName)
    {
        return compilation.GetSymbolsWithName(x => x == symbolName).Single();
    }

    public static string GetName(ClassDeclarationSyntax parameter)
    {
        return parameter.Identifier.ValueText;
    }
    public static string? GetName(TypeSyntax type)
    {
        return type is IdentifierNameSyntax identifier ? identifier.Identifier.ValueText : null;
    }
    public static string GetName(ParameterSyntax parameter)
    {
        return parameter.Identifier.ValueText;
    }
    public static string GetName(ITypeSymbol symbol)
    {
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

public record QueryType
{
    public string Namespace { get; }
    public string FullyQualifiedTypeName { get; }
    public string TypeName { get; }

    public QueryType(
        string @namespace,
        string fullyQualifiedTypeName,
        string typeName
    )
    {
        Namespace = @namespace;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        TypeName = typeName;
    }

    public static QueryType FromSymbol(ITypeSymbol symbol)
    {
        return new QueryType(
            @namespace: Query.GetNamespace(symbol),
            fullyQualifiedTypeName: Query.GetFullyQualifiedTypeName(symbol),
            typeName: Query.GetName(symbol)
        );
    }
}

public sealed record QueryEntity : QueryType
{
    public IReadOnlyList<QueryProperty> Properties { get; }

    public QueryEntity(
        string @namespace,
        string fullyQualifiedEntityName,
        string entityName,
        IReadOnlyList<QueryProperty> properties
    ) : base(@namespace, fullyQualifiedEntityName, entityName)
    {
        Properties = properties;
    }

    public static QueryEntity FromSymbol(ISymbol? symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Missing entity symbol.");
        }
        var properties = namedSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Select(QueryProperty.FromSymbol)
            .Where(x => x.PropertyName != "EqualityContract")
            .ToList();
        return new QueryEntity(
            @namespace: Query.GetNamespace(namedSymbol),
            fullyQualifiedEntityName: Query.GetFullyQualifiedTypeName(namedSymbol),
            entityName: Query.GetName(namedSymbol),
            properties
        );
    }

    public static QueryEntity FromAttribute(AttributeSyntax attribute, Compilation compilation)
    {
        // take from first argument that must be as of now in form of "typeof(<EntityType>)"
        var firstArgument = attribute.ArgumentList?.Arguments.FirstOrDefault();
        if (firstArgument is null)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Cannot find first attribute argument of 'typeof(<EntityType>)'");
        }
        var entityTypeOf = (IdentifierNameSyntax)((TypeOfExpressionSyntax)firstArgument.Expression).Type;
        var entityTypeSymbol = Query.GetSymbol(compilation, entityTypeOf.Identifier.ValueText);
        if (entityTypeSymbol is not INamedTypeSymbol namedSymbol)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Missing entity symbol.");
        }

        var properties = namedSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Select(QueryProperty.FromSymbol)
            .Where(x => x.PropertyName != "EqualityContract")
            .ToList();
        return new QueryEntity(
            @namespace: Query.GetNamespace(namedSymbol),
            fullyQualifiedEntityName: Query.GetFullyQualifiedTypeName(namedSymbol),
            entityName: Query.GetName(namedSymbol),
            properties
        );
    }
}

public sealed record QueryProperty
{
    public string PropertyName { get; }
    public QueryType PropertyType { get; }

    public QueryProperty(
        string propertyName,
        QueryType propertyType
    )
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public static QueryProperty FromSymbol(IPropertySymbol symbol)
    {
        return new QueryProperty(
            propertyName: symbol.Name,
            propertyType: QueryType.FromSymbol(symbol.Type)
        );
    }
}
