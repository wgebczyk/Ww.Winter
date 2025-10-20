namespace Ww.Winter.Generator;

public static class Helpers
{
    public static string ToSafeFileName(string typeName, string generatorName)
    {
        return $"{typeName}_{generatorName}.g.cs".Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '.')
            .Replace(' ', '_');
    }
}
