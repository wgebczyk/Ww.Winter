using System.Collections.Immutable;

namespace Ww.Winter.Generator.Primitives;

public sealed record FilterProperty(
    ImmutableArray<PropertyModel> Properties,
    FilterComparison Comparison
);
