using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace ConfigurationApi.V1.Infrastructure
{
    public static class S3InitilisationExtensions
    {
        public static void ConfigureS3(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            var url = configuration.GetValue<string>("CONFIGURATION_S3_URL");
            if (!string.IsNullOrEmpty(url))
            {
                // This is for when running locally / using localstack
                services.AddSingleton<IAmazonS3>(sp =>
                {
                    var clientConfig = new AmazonS3Config() { ServiceURL = url, ForcePathStyle = true };
                    var credentials = new Amazon.Runtime.BasicAWSCredentials("xxx", "xxx");
                    return new AmazonS3Client(credentials, clientConfig);
                });
            }
            else
            {
                services.TryAddSingleton<IAmazonS3>(sp =>
                {
                    var clientConfig = new AmazonS3Config() { RegionEndpoint = RegionEndpoint.EUWest2 };
                    return new AmazonS3Client(clientConfig);
                });
            }
        }
    }
}
