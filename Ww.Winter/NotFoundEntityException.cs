using System;

namespace Ww.Winter;

public sealed class NotFoundEntityException(string entityName, string context)
    : Exception($"Entity '{entityName}' not found due to {context}")
{
}
