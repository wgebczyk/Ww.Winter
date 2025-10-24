using System;

namespace Ww.Winter;

[AttributeUsage(AttributeTargets.Class)]
public sealed class QueryableFilterAttribute(Type entityType) : Attribute
{
    public const string FullTypeName = "Ww.Winter.QueryableFilterAttribute";

    public Type EntityType { get; } = entityType;
}
