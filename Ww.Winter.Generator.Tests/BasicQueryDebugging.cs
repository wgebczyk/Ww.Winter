using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ww.Winter.Generator.BasicQueries;

namespace Ww.Winter.Generator.Tests;

public sealed class BasicQueryDebugging
{
    [Fact]
    public void DebugIt()
    {
        var source =
        @"""
        using Ww.Winter;

        namespace Ww.Winter.Generator.Tests;

        public partial class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [BasicQuery(typeof(Person), ""GetPersonByFirstNameOrLastName"", UseBaseQuery = ""BaseQuery"")]
        [BasicQuery(typeof(Person), ""GetPersonByFirstNameAndLastName"")]
        public partial class PersonQueries
        {
        }
        """;

        var driver = DriveBasicQuery(source);
        var result = driver.GetRunResult();
    }

    public static GeneratorDriver DriveBasicQuery(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Ww.Winter.Generator.Tests__",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(BasicQueryAttribute).Assembly.Location)]
        );

        var generator = new BasicQueryIncrementalGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return driver;
    }
}
