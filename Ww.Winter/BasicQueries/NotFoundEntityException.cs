using System;

namespace Ww.Winter;

public sealed class NotFoundEntityException : Exception
{
    public NotFoundEntityException(string entityName, string context) : base($"Entity '{entityName}' not found due to {context}")
    {
    }
}
