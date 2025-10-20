using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ww.Winter;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class QueryAttribute: Attribute
{
    public const string FullTypeName = "Ww.Winter.QueryAttribute";

    public Type EntityType { get; }

    public QueryAttribute(Type entityType)
    {
        EntityType = entityType;
    }
}

public sealed record SortProperty(
    string PropertyName,
    ListSortDirection Direction
);
public sealed record SortParams(
    IReadOnlyList<SortProperty> Properties
);

public sealed record PaginationParams(
    int? Skip,
    int? Take
)
{
    public bool IsDefined => Skip.HasValue || Take.HasValue;
}
