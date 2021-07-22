using System;
using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationApi.Tests
{
    public class AwsMockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        public IAmazonS3 S3Client { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();

            builder.ConfigureServices(services =>
            {
                var url = "http://localhost:4566";

                services.AddSingleton<IAmazonS3>(sp =>
                {
                    var clientConfig = new AmazonS3Config() { ServiceURL = url, ForcePathStyle = true};
                    return new AmazonS3Client(clientConfig);
                });

                var serviceProvider = services.BuildServiceProvider();
                S3Client = serviceProvider.GetRequiredService<IAmazonS3>();
            });
        }
    }
}
