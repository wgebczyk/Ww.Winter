using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ww.Winter.Generator.Model;

namespace Ww.Winter.Generator.Parsing;

public sealed class FilterPropertyIdentifierParser
{
    public bool TryParse(EntityModel entity, string propertyName, [MaybeNullWhen(false)] out FilterProperty filterProperty)
    {
        var filterPropertyName = propertyName;
        var matchingProperties = entity.Properties.Where(x => filterPropertyName.StartsWith(x.Name)).ToArray();
        if (matchingProperties.Length == 0)
        {
            filterProperty = null;
            return false;
        }
        if (matchingProperties.Length > 1)
        {
            filterProperty = null;
            return false;
        }
        var entityProperty = matchingProperties.Single();
        var comparisonText = filterPropertyName.Substring(entityProperty.Name.Length);
        if (comparisonText == "")
        {
            filterProperty = new FilterProperty([entityProperty], FilterComparison.Equals);
            return true;
        }
        else
        {
            var comparison = FilterComparisonMappings.Single(x => x.Suffix == comparisonText).Comparison;
            filterProperty = new FilterProperty([entityProperty], comparison);
            return true;
        }
    }

    private static readonly (string Suffix, FilterComparison Comparison)[] FilterComparisonMappings =
    [
        ("Equals", FilterComparison.Equals),
        ("Is", FilterComparison.Equals),
        ("NotEquals", FilterComparison.NotEquals),
        ("IsNot", FilterComparison.NotEquals),
        ("GreaterThanOrEqual", FilterComparison.GreaterThanOrEqual),
        ("From", FilterComparison.GreaterThanOrEqual),
        ("GreaterThan", FilterComparison.GreaterThan),
        ("LessThanOrEqual", FilterComparison.LessThanOrEqual),
        ("To", FilterComparison.LessThanOrEqual),
        ("LessThan", FilterComparison.LessThan),
        ("Contains", FilterComparison.Contains),
        ("NotContains", FilterComparison.NotContains),
        ("Fragment", FilterComparison.Contains),
        ("StartsWith", FilterComparison.StartsWith),
        ("Prefix", FilterComparison.StartsWith),
        ("NotStartsWith", FilterComparison.NotStartsWith),
        ("EndsWith", FilterComparison.EndsWith),
        ("Suffix", FilterComparison.EndsWith),
        ("NotEndsWith", FilterComparison.NotEndsWith),
    ];
}
