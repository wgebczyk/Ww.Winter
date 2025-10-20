using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ww.Winter.Generator.BasicQueries;

public enum FilterOperator
{
    And,
    Or,
}
public enum FilterComparison
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
}

public sealed record BasicEntityQuery
{
    public BasicQueryEntity EntityType { get; }
    public bool IsSingleEntityResult { get; }
    public FilterOperator FilterOperator { get; }
    public IReadOnlyList<QueryFilterProperty> FilterProperties { get; }

    public BasicEntityQuery(BasicQueryEntity entityType, bool isSingleEntityResult, FilterOperator filterOperator, IReadOnlyList<QueryFilterProperty> filterProperties)
    {
        EntityType = entityType;
        IsSingleEntityResult = isSingleEntityResult;
        FilterOperator = filterOperator;
        FilterProperties = filterProperties;
    }
}
public sealed record QueryFilterProperty
{
    public BasicQueryProperty Property { get; }
    public FilterComparison Comparison { get; }

    public QueryFilterProperty(BasicQueryProperty property, FilterComparison comparison)
    {
        Property = property;
        Comparison = comparison;
    }
}

public sealed class BasicEntityQueryIdentifierParser
{
    public BasicEntityQuery? Parse(BasicQueryEntity entityType, string queryIdentifier)
    {
        var byIndex = queryIdentifier.IndexOf("By", StringComparison.Ordinal);
        if (byIndex == -1)
        {
            return null;
        }

        var operationPart = queryIdentifier.Substring(0, byIndex);
        var filterPart = queryIdentifier.Substring(byIndex + 2);
        if (!TryCutPrefix(operationPart, out var isSingleEntityResult, out var _))
        {
            return null;
        }

        return ParseCore(entityType, isSingleEntityResult, filterPart);
    }

    private static string[] SingleEntityResultPrefixes =
    [
        "Get",
        "Single",
    ];
    private static string[] MultiEntityResultPrefixes =
    [
        "Find",
        "Filter",
    ];
    private static bool TryCutPrefix(
        string operationPart,
        [MaybeNullWhen(false)] out bool isSingleEntityResult,
        [MaybeNullWhen(false)] out string? entityPart)
    {
        foreach (var prefix in SingleEntityResultPrefixes)
        {
            if (operationPart.StartsWith(prefix))
            {
                isSingleEntityResult = true;
                entityPart = operationPart.Substring(prefix.Length);
                return true;
            }
        }
        foreach (var prefix in MultiEntityResultPrefixes)
        {
            if (operationPart.StartsWith(prefix))
            {
                isSingleEntityResult = false;
                entityPart = operationPart.Substring(prefix.Length);
                return true;
            }
        }
        isSingleEntityResult = default;
        entityPart = null;
        return false;
    }

    private BasicEntityQuery? ParseCore(BasicQueryEntity entityType, bool isSingleEntityResult, string filterPart)
    {
        if (!TryCutProperty(entityType, filterPart, out var currentProperty, out var remainingFilterPart))
        {
            return null;
        }
        if (!TryCutFilterComparison(remainingFilterPart, out var currentComparison, out var remainingFilterPart2))
        {
            currentComparison = FilterComparison.Equals;
            remainingFilterPart2 = remainingFilterPart;
        }
        FilterOperator? filterOperator = null;
        var filterProperties = new List<QueryFilterProperty> { new QueryFilterProperty(currentProperty, currentComparison) };

        filterPart = remainingFilterPart2;
        while (!string.IsNullOrEmpty(filterPart))
        {
            if (!TryCutFilterOperator(filterPart, out var currentFilterOperator, out var remainingFilterPart1))
            {
                return null;
            }
            if (filterOperator is null)
            {
                filterOperator = currentFilterOperator;
            }
            else if (filterOperator != currentFilterOperator)
            {
                // Mixed operators are not supported
                return null;
            }
            if (!TryCutProperty(entityType, remainingFilterPart1, out currentProperty, out var remainingFilterPart3))
            {
                return null;
            }
            if (!TryCutFilterComparison(remainingFilterPart3, out currentComparison, out var remainingFilterPart4))
            {
                currentComparison = FilterComparison.Equals;
                remainingFilterPart4 = remainingFilterPart3;
            }

            filterProperties.Add(new QueryFilterProperty(currentProperty, currentComparison));

            filterPart = remainingFilterPart4;
        }

        return new BasicEntityQuery(
            entityType,
            isSingleEntityResult,
            filterOperator ?? FilterOperator.And,
            filterProperties
        );
    }

    private bool TryCutProperty(BasicQueryEntity entityType, string filterPart, [MaybeNullWhen(false)] out BasicQueryProperty property, [MaybeNullWhen(false)] out string remainingFilterPart)
    {
        foreach (var entityProperty in entityType.Properties)
        {
            if (filterPart.StartsWith(entityProperty.PropertyName, StringComparison.Ordinal))
            {
                property = entityProperty;
                remainingFilterPart = filterPart.Substring(property.PropertyName.Length);
                return true;
            }
        }
        property = default!;
        remainingFilterPart = default!;
        return false;
    }

    private static readonly (string Prefix, FilterOperator Operator)[] FilterOperatorMappings =
    [
        ("And", FilterOperator.And),
        ("Or", FilterOperator.Or)
    ];
    private static bool TryCutFilterOperator(string filterPart, [MaybeNullWhen(false)] out FilterOperator filterOperator, [MaybeNullWhen(false)] out string remainingFilterPart)
    {
        foreach (var entry in FilterOperatorMappings)
        {
            if (filterPart.StartsWith(entry.Prefix, StringComparison.Ordinal))
            {
                filterOperator = entry.Operator;
                remainingFilterPart = filterPart.Substring(entry.Prefix.Length);
                return true;
            }
        }
        filterOperator = default;
        remainingFilterPart = default!;
        return false;
    }

    private static readonly (string Prefix, FilterComparison Comparison)[] FilterComparisonMappings =
    [
        ("Equals", FilterComparison.Equals),
        ("NotEquals", FilterComparison.NotEquals),
        ("GreaterThanOrEqual", FilterComparison.GreaterThanOrEqual),
        ("GreaterThan", FilterComparison.GreaterThan),
        ("LessThanOrEqual", FilterComparison.LessThanOrEqual),
        ("LessThan", FilterComparison.LessThan),
        ("Contains", FilterComparison.Contains),
        ("StartsWith", FilterComparison.StartsWith),
        ("EndsWith", FilterComparison.EndsWith),
    ];
    private static bool TryCutFilterComparison(string filterPart, [MaybeNullWhen(false)] out FilterComparison filterComparison, [MaybeNullWhen(false)] out string remainingFilterPart)
    {
        foreach (var entry in FilterComparisonMappings)
        {
            if (filterPart.StartsWith(entry.Prefix, StringComparison.Ordinal))
            {
                filterComparison = entry.Comparison;
                remainingFilterPart = filterPart.Substring(entry.Prefix.Length);
                return true;
            }
        }
        filterComparison = default;
        remainingFilterPart = default;
        return false;
    }
}
