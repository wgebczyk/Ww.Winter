using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ww.Winter.Generator.BasicQueries;

public record BasicQueryToGenerate
{
    public string Namespace { get; }
    public string FullyQualifiedClassName { get; }
    public string ClassName { get; }
    public IReadOnlyList<BasicQuery> Queries { get; }

    public BasicQueryToGenerate(
        string @namespace,
        string fullyQualifiedClassName,
        string className,
        IReadOnlyList<BasicQuery> queries
    )
    {
        Namespace = @namespace;
        FullyQualifiedClassName = fullyQualifiedClassName;
        ClassName = className;
        Queries = queries;
    }
}
public record BasicQuery
{
    public string MethodName { get; }
    public BasicQueryEntity Entity { get; }

    public BasicQuery(
        string methodName,
        BasicQueryEntity entity
    )
    {
        MethodName = methodName;
        Entity = entity;
    }

    public static BasicQuery FromAttribute(AttributeSyntax attribute, Compilation compilation)
    {
        var arguments = attribute.ArgumentList?.Arguments.ToArray() ?? [];
        if (arguments.Length != 2)
        {
            throw new Exception("INTERNAL ERROR: Expected exactly 2 arguments for attribute.");
        }
        var entityTypeOf = GetName(((TypeOfExpressionSyntax)arguments[0].Expression).Type)!;
        var entityType = GetSymbol(compilation, entityTypeOf);

        return new BasicQuery(
            methodName: (arguments[1].Expression as LiteralExpressionSyntax)?.Token.ValueText!,
            entity: BasicQueryEntity.FromSymbol(entityType)
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
public record BasicQueryType
{
    public string Namespace { get; }
    public string FullyQualifiedTypeName { get; }
    public string TypeName { get; }

    public BasicQueryType(
        string @namespace,
        string fullyQualifiedTypeName,
        string typeName
    )
    {
        Namespace = @namespace;
        FullyQualifiedTypeName = fullyQualifiedTypeName;
        TypeName = typeName;
    }

    public static BasicQueryType FromSymbol(ITypeSymbol symbol)
    {
        return new BasicQueryType(
            @namespace: BasicQuery.GetNamespace(symbol),
            fullyQualifiedTypeName: BasicQuery.GetFullyQualifiedTypeName(symbol),
            typeName: BasicQuery.GetName(symbol)
        );
    }
}
public record BasicQueryEntity : BasicQueryType
{
    public IReadOnlyList<BasicQueryProperty> Properties { get; }

    public BasicQueryEntity(
        string @namespace,
        string fullyQualifiedEntityName,
        string entityName,
        IReadOnlyList<BasicQueryProperty> properties
    ) : base(@namespace, fullyQualifiedEntityName, entityName)
    {
        Properties = properties;
    }

    public static BasicQueryEntity FromSymbol(ISymbol? symbol)
    {
        if (symbol is not INamedTypeSymbol namedSymbol)
        {
            throw new InvalidOperationException("INTERNAL ERROR: Missing entity symbol.");
        }
        return new BasicQueryEntity(
            @namespace: BasicQuery.GetNamespace(namedSymbol),
            fullyQualifiedEntityName: BasicQuery.GetFullyQualifiedTypeName(namedSymbol),
            entityName: BasicQuery.GetName(namedSymbol),
            properties: namedSymbol.GetMembers().OfType<IPropertySymbol>().Select(BasicQueryProperty.FromSymbol).ToList()
        );
    }
}
public record BasicQueryProperty
{
    public string PropertyName { get; }
    public BasicQueryType PropertyType { get; }

    public BasicQueryProperty(
        string propertyName,
        BasicQueryType propertyType
    )
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public static BasicQueryProperty FromSymbol(IPropertySymbol symbol)
    {
        return new BasicQueryProperty(
            propertyName: symbol.Name,
            propertyType: BasicQueryType.FromSymbol(symbol.Type)
        );
    }
}
