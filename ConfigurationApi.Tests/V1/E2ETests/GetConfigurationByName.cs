using ConfigurationApi.Tests;
using ConfigurationApi.V1.Domain;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Xunit;
using FluentAssertions;

[Collection("Aws collection")]
public class GetConfigurationByName
{
    private readonly AwsIntegrationTests _awsFixture;
    public HttpClient Client { get; set; }

    public GetConfigurationByName(AwsIntegrationTests awsFixture)
    {
        _awsFixture = awsFixture;
        Client = _awsFixture.Client;
    }

    [Fact]
    public async Task GivenAConfigurationTypeWhenQueryingControllerReturnsJsonFile()
    {
        var result = await Client.GetAsync(new Uri("api/v1/configuration?types=First", UriKind.Relative)).ConfigureAwait(false);
        var listOfConfigurations = JsonConvert.DeserializeObject<List<ApiConfiguration>>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
        listOfConfigurations.Count.Should().Be(1);
    }
}
