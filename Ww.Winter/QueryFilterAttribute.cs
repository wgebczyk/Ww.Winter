using System;

namespace Ww.Winter;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class QueryFilterAttribute(Type entityType, Type filterType) : Attribute
{
    public const string FullTypeName = "Ww.Winter.QueryFilterAttribute";

    public Type EntityType { get; } = entityType;
    public Type FilterType { get; } = filterType;
}
