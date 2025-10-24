using System.Collections.Immutable;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.BasicQueries;

public sealed record BasicEntityQuery(
    EntityModel Entity,
    bool IsSingleEntityResult,
    FilterOperator FilterOperator,
    ImmutableArray<FilterProperty> FilterProperties
);
