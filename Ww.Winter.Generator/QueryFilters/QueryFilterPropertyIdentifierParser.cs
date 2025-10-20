using System;
using System.Linq;

namespace Ww.Winter.Generator.QueryFilters;

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

public sealed record QueryFilterProperty
{
    public QueryProperty Property { get; }
    public FilterComparison Comparison { get; }

    public QueryFilterProperty(QueryProperty property, FilterComparison comparison)
    {
        Property = property;
        Comparison = comparison;
    }
}

public sealed class QueryFilterPropertyIdentifierParser
{
    public QueryFilterProperty? Parse(QueryFilterEntity entityType, QueryProperty filterProperty)
    {
        var filterPropertyName = filterProperty.PropertyName;
        var matchingProperties = entityType.Properties.Where(x => filterPropertyName.StartsWith(x.PropertyName)).ToArray();
        if (matchingProperties.Length == 0)
        {
            throw new Exception($"No matching properties found. Entity: {entityType.FullyQualifiedTypeName}, FilterProperty: {filterPropertyName}");
        }
        if (matchingProperties.Length > 1)
        {
            throw new Exception("Multiple matching properties found.  Entity: {entityType.FullyQualifiedTypeName}, FilterProperty: {filterPropertyName}");
        }
        var entityProperty = matchingProperties.Single();
        var comparisonText = filterPropertyName.Substring(entityProperty.PropertyName.Length);
        if (comparisonText == "")
        {
            return new QueryFilterProperty(entityProperty, FilterComparison.Equals);
        }
        else
        {
            var comparison = FilterComparisonMappings.Single(x => x.Suffix == comparisonText).Comparison;
            return new QueryFilterProperty(entityProperty, comparison);
        }
    }

    private static readonly (string Suffix, FilterComparison Comparison)[] FilterComparisonMappings =
    [
        ("Equals", FilterComparison.Equals),
        ("NotEquals", FilterComparison.NotEquals),
        ("GreaterThanOrEqual", FilterComparison.GreaterThanOrEqual),
        ("From", FilterComparison.GreaterThanOrEqual),
        ("GreaterThan", FilterComparison.GreaterThan),
        ("LessThanOrEqual", FilterComparison.LessThanOrEqual),
        ("To", FilterComparison.LessThanOrEqual),
        ("LessThan", FilterComparison.LessThan),
        ("Contains", FilterComparison.Contains),
        ("Fragment", FilterComparison.Contains),
        ("StartsWith", FilterComparison.StartsWith),
        ("Prefix", FilterComparison.StartsWith),
        ("EndsWith", FilterComparison.EndsWith),
        ("Suffix", FilterComparison.EndsWith),
    ];
}
