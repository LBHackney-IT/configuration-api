using System.Linq;
using ConfigurationApi.V1.Domain;
using FluentAssertions;
using Xunit;

namespace ConfigurationApi.Tests.V1.Domain
{
    public class ApiConfigurationTests
    {
        private string _configurationArray =
            "{ \"Type\": \"First\", \"Configuration\": { \"ApiUrl\": \"https://first.gov.uk/\" }, \"FeatureToggles\": { \"CreatePerson\": true, \"EditPerson\": true } }";

        [Fact]
        public void GivenAnArrayOfResultsWhenDeserializingShouldDoSoCorrectly()
        {
            // Arrange + Act
            var result = ApiConfiguration.Create(_configurationArray);

            // Assert
            result.Type.Should().Be("First");
            result.Configuration.ApiUrl.Should().Be("https://first.gov.uk/");
            result.FeatureToggles.First().Key.Should().Be("CreatePerson");
            result.FeatureToggles.First().Value.Should().Be(true);
            result.FeatureToggles.Skip(1).First().Key.Should().Be("EditPerson");
            result.FeatureToggles.Skip(1).First().Value.Should().Be(true);
        }
    }
}
