using System.Collections.Immutable;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Queries;

public sealed record QueryToGenerate(
    TypeModel OwnedBy,
    ImmutableHashSet<string> OwnedByMethods,
    ImmutableArray<Query> Queries
);
