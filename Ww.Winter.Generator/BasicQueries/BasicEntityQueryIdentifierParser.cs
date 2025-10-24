using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public sealed class BasicEntityQueryIdentifierParser
{
    public bool TryParse(EntityModel entity, string queryIdentifier, [MaybeNullWhen(false)] out BasicEntityQuery query)
    {
        var byIndex = queryIdentifier.IndexOf("By", StringComparison.Ordinal);
        if (byIndex == -1)
        {
            query = null;
            return false;
        }

        var operationPart = queryIdentifier.Substring(0, byIndex);
        var filterPart = queryIdentifier.Substring(byIndex + 2);
        if (!TryCutPrefix(operationPart, out var isSingleEntityResult, out var _))
        {
            query = null;
            return false;
        }

        query = ParseCore(entity, isSingleEntityResult, filterPart);
        return query is not null;
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

    private BasicEntityQuery? ParseCore(EntityModel entity, bool isSingleEntityResult, string filterPart)
    {
        if (!TryCutProperty(entity, filterPart, out var currentProperty, out var remainingFilterPart))
        {
            return null;
        }
        if (!TryCutFilterComparison(remainingFilterPart, out var currentComparison, out var remainingFilterPart2))
        {
            currentComparison = FilterComparison.Equals;
            remainingFilterPart2 = remainingFilterPart;
        }
        FilterOperator? filterOperator = null;
        var filterProperties = new List<FilterProperty> { new FilterProperty([currentProperty], currentComparison) };

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
            if (!TryCutProperty(entity, remainingFilterPart1, out currentProperty, out var remainingFilterPart3))
            {
                return null;
            }
            if (!TryCutFilterComparison(remainingFilterPart3, out currentComparison, out var remainingFilterPart4))
            {
                currentComparison = FilterComparison.Equals;
                remainingFilterPart4 = remainingFilterPart3;
            }

            filterProperties.Add(new FilterProperty([currentProperty], currentComparison));

            filterPart = remainingFilterPart4;
        }

        return new BasicEntityQuery(
            entity,
            isSingleEntityResult,
            filterOperator ?? FilterOperator.And,
            [..filterProperties]
        );
    }

    private bool TryCutProperty(EntityModel entity, string filterPart, [MaybeNullWhen(false)] out PropertyModel property, [MaybeNullWhen(false)] out string remainingFilterPart)
    {
        foreach (var entityProperty in entity.Properties)
        {
            if (filterPart.StartsWith(entityProperty.Name, StringComparison.Ordinal))
            {
                property = entityProperty;
                remainingFilterPart = filterPart.Substring(property.Name.Length);
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
