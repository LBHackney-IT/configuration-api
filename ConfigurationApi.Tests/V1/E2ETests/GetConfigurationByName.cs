using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConfigurationApi.V1.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;
using Xunit;

namespace ConfigurationApi.Tests.V1.E2ETests
{
    [Collection("Aws collection")]
    public class GetConfigurationByName
    {
        private readonly AwsIntegrationTests<Startup> _awsFixture;
        public HttpClient Client { get; set; }

        public GetConfigurationByName(AwsIntegrationTests<Startup> awsFixture)
        {
            _awsFixture = awsFixture;
            Client = _awsFixture.Client;
        }

        [Fact]
        public async Task GivenAConfigurationTypeWhenQueryingControllerReturnsJsonFile()
        {
            // Arrange + Act
            var result = await Client.GetAsync(new Uri("api/v1/configuration?types=First", UriKind.Relative)).ConfigureAwait(false);

            // Assert
            var listOfConfigurations = JsonConvert.DeserializeObject<List<ApiConfiguration>>(result.Content.ReadAsStringAsync().Result);
            listOfConfigurations.Count.Should().Be(1);
        }
    }
}
