using System.Collections.Immutable;

namespace Ww.Winter.Generator.Model;

public sealed record FilterProperty(
    ImmutableArray<PropertyModel> Properties,
    FilterComparison Comparison
);
