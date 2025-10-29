using FluentAssertions;
using Ww.Winter.Generator.BasicQueries;

namespace Ww.Winter.Generator.Tests.BasicQueries;

public sealed class BasicQueryIncrementalGeneratorShould
{
    [Fact]
    public void HandleKitchenSinkExample()
    {
        var source =
        @"
        using Ww.Winter;

        namespace Ww.Winter.Generator.Tests;

        public partial class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [BasicQuery(typeof(Person), ""GetPersonByFirstNameOrLastName"", UseBaseQueryExpression = ""BaseQuery()"")]
        [BasicQuery(typeof(Person), ""GetPersonByFirstNameAndLastName"")]
        public partial class PersonQueries
        {
        }
        ";

        var driver = CodeGeneration.DriveSource(new BasicQueryIncrementalGenerator(), source);
        var result = driver.GetRunResult();

        var expectedLines =
            """
            using Microsoft.EntityFrameworkCore;
            using Ww.Winter;

            #nullable enable

            namespace Ww.Winter.Generator.Tests;

            partial class PersonQueries
            {
                public async Task<Person> GetPersonByFirstNameOrLastName(
                    string? firstName,
                    string? lastName,
                    CancellationToken cancellationToken)
                {
                    var query = BaseQuery()
                        .TagWith("GetPersonByFirstNameOrLastName");

                    if (firstName is not null)
                    {
                        query = query.Where(e => e.FirstName == firstName);
                    }
                    if (lastName is not null)
                    {
                        query = query.Where(e => e.LastName == lastName);
                    }

                    var result = await query.SingleOrDefaultAsync(cancellationToken);
                    if (result is null)
                    {
                        throw new NotFoundEntityException("Person", "Id lookup failure");
                    }
                    return result;
                }
                public async Task<Person> GetPersonByFirstNameAndLastName(
                    string? firstName,
                    string? lastName,
                    CancellationToken cancellationToken)
                {
                    var query = this.dbContext.Persons
                        .AsNoTracking()
                        .TagWith("GetPersonByFirstNameAndLastName");

                    if (firstName is not null)
                    {
                        query = query.Where(e => e.FirstName == firstName);
                    }
                    if (lastName is not null)
                    {
                        query = query.Where(e => e.LastName == lastName);
                    }

                    var result = await query.SingleOrDefaultAsync(cancellationToken);
                    if (result is null)
                    {
                        throw new NotFoundEntityException("Person", "Id lookup failure");
                    }
                    return result;
                }
            }
            """
            .Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("{") && !x.StartsWith("}") && !x.StartsWith("break"))
            .Order()
            .Distinct()
            .ToList();

        var generatedSource = result.Results.First().GeneratedSources.First().SourceText.ToString();
        foreach (var expectedLine in expectedLines)
        {
            generatedSource.Should().Contain(expectedLine);
        }
    }
}
