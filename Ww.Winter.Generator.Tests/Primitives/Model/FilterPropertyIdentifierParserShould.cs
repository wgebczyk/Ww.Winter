using FluentAssertions;
using Ww.Winter.Generator.Primitives;

namespace Ww.Winter.Generator.Tests.Primitives.Model;

public sealed class FilterPropertyIdentifierParserShould
{
    [Theory]
    [InlineData("FirstName", "FirstName", FilterComparison.Equals)]
    [InlineData("FirstNameIs", "FirstName", FilterComparison.Equals)]
    [InlineData("FirstNameEquals", "FirstName", FilterComparison.Equals)]
    [InlineData("FirstNameIsNot", "FirstName", FilterComparison.NotEquals)]
    [InlineData("FirstNameNotEquals", "FirstName", FilterComparison.NotEquals)]
    [InlineData("FirstNameContains", "FirstName", FilterComparison.Contains)]
    [InlineData("FirstNameFragment", "FirstName", FilterComparison.Contains)]
    [InlineData("FirstNameNotContains", "FirstName", FilterComparison.NotContains)]
    [InlineData("LastNameStartsWith", "LastName", FilterComparison.StartsWith)]
    [InlineData("LastNamePrefix", "LastName", FilterComparison.StartsWith)]
    [InlineData("LastNameNotStartsWith", "LastName", FilterComparison.NotStartsWith)]
    [InlineData("LastNameEndsWith", "LastName", FilterComparison.EndsWith)]
    [InlineData("LastNameSuffix", "LastName", FilterComparison.EndsWith)]
    [InlineData("LastNameNotEndsWith", "LastName", FilterComparison.NotEndsWith)]

    [InlineData("AddressCountGreaterThanOrEqual", "AddressCount", FilterComparison.GreaterThanOrEqual)]
    [InlineData("AddressCountFrom", "AddressCount", FilterComparison.GreaterThanOrEqual)]
    [InlineData("AddressCountGreaterThan", "AddressCount", FilterComparison.GreaterThan)]
    [InlineData("AddressCountLessThanOrEqual", "AddressCount", FilterComparison.LessThanOrEqual)]
    [InlineData("AddressCountTo", "AddressCount", FilterComparison.LessThanOrEqual)]
    [InlineData("AddressCountLessThan", "AddressCount", FilterComparison.LessThan)]
    public void ParseSimplePropertyWithComparison(string propertyToParse, string expectedPropertyName, FilterComparison expectedComparison)
    {
        var result = new FilterPropertyIdentifierParser().TryParse(CreateBasicEntityModel(), propertyToParse, out var filterProperty);

        result.Should().BeTrue();
        filterProperty.Should().NotBeNull();
        filterProperty!.Properties.Should().HaveCount(1);
        filterProperty!.Properties[0].Name.Should().Be(expectedPropertyName);
        filterProperty!.Comparison.Should().Be(expectedComparison);
    }

    private static EntityModel CreateBasicEntityModel()
    {
        return new EntityModel(
            new TypeModel("SomeNamespace", "BasicEntity", "SomeNamespace.BasicEntity", false, []),
            [
                new PropertyModel("FirstName", new PropertyTypeModel("String", false, null)),
                new PropertyModel("LastName", new PropertyTypeModel("String", false, null)),
                new PropertyModel("AddressCount", new PropertyTypeModel("Int32", false, null)),
            ], []
        );
    }
}
