using Amazon.S3;
using ConfigurationApi.V1.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ConfigurationApi.Tests
{
    public class AwsMockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        public IAmazonS3 S3Client { get; set; }
        private IConfiguration _configuration;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b =>
            {
                b.AddEnvironmentVariables();
                _configuration = b.Build();
            }).UseStartup<Program>();

            builder.ConfigureServices(services =>
            {
                services.ConfigureS3(_configuration);

                var serviceProvider = services.BuildServiceProvider();
                S3Client = serviceProvider.GetRequiredService<IAmazonS3>();
            });
        }
    }
}
