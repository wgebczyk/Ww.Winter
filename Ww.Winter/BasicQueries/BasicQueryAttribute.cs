using System;

namespace Ww.Winter;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class BasicQueryAttribute: Attribute
{
    public const string FullTypeName = "Ww.Winter.BasicQueryAttribute";

    public Type EntityType { get; }
    public string QueryIdentifier { get; }

    public BasicQueryAttribute(Type entityType, string queryIdentifier)
    {
        EntityType = entityType;
        QueryIdentifier = queryIdentifier;
    }
}
