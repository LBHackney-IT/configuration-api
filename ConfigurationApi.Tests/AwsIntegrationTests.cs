using System;
using System.Net.Http;
using Amazon.S3;
using Amazon.S3.Model;
using Xunit;

namespace ConfigurationApi.Tests
{
    public class AwsIntegrationTests<TStartup> : IDisposable where TStartup : class
    {
        public HttpClient Client { get; private set; }
        public IAmazonS3 S3Client => _factory?.S3Client;

        private readonly AwsMockWebApplicationFactory<TStartup> _factory;

        public AwsIntegrationTests()
        {
            _factory = new AwsMockWebApplicationFactory<TStartup>();

            EnsureEnvVarConfigured("CONFIGURATION_S3_BUCKETNAME", "configuration-api-configurations");
            EnsureEnvVarConfigured("CONFIGURATION_S3_URL", "http://localhost:4566");

            Client = _factory.CreateClient();

            CreateS3File();
        }

        private void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        private void CreateS3File()
        {
            var bucketName = Environment.GetEnvironmentVariable("CONFIGURATION_S3_BUCKETNAME");
            var bucket = S3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName
            }).GetAwaiter().GetResult();

            var testString =
                "{ \"Type\": \"First\", \"Configuration\": { \"ApiUrl\": \"https://first.gov.uk/\" }, \"FeatureToggles\": { \"CreatePerson\": true, \"EditPerson\": true } }";

            var putRequest = new PutObjectRequest();
            putRequest.Key = "First";
            putRequest.BucketName = bucketName;
            putRequest.ContentType = "application/json";
            putRequest.ContentBody = testString;


            PutObjectResponse response = S3Client.PutObjectAsync(putRequest).GetAwaiter().GetResult();
        }

        public void Dispose()
        {

        }
    }


    [CollectionDefinition("Aws collection", DisableParallelization = true)]
    public class DynamoDbCollection : ICollectionFixture<AwsIntegrationTests<Startup>>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
