namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit
    {
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    public class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }
}
