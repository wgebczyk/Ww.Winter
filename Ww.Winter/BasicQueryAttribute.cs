using System;

namespace Ww.Winter;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class BasicQueryAttribute(Type entityType, string queryIdentifier) : Attribute
{
    public const string FullTypeName = "Ww.Winter.BasicQueryAttribute";

    public Type EntityType { get; } = entityType;
    public string QueryIdentifier { get; } = queryIdentifier;

    public string? UseBaseQueryExpression { get; set; }
}
