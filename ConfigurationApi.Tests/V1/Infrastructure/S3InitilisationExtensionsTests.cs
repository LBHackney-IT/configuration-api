using Amazon.S3;
using ConfigurationApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConfigurationApi.Tests.V1.Infrastructure
{
    public class S3InitilisationExtensionsTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;

        private const string ConfigKey = "CONFIGURATION_S3_URL";
        private const string S3Url = "http://somedomain:4566";

        public S3InitilisationExtensionsTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            ConfigureConfig(_mockConfiguration, S3Url);
        }

        private static void ConfigureConfig(Mock<IConfiguration> mockConfig, string url)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(x => x.Key).Returns(ConfigKey);
            section.Setup(x => x.Value).Returns(url);
            mockConfig.Setup(x => x.GetSection(ConfigKey))
                              .Returns(section.Object);
        }

        [Fact]
        public void ConfigureS3TestNullServicesThrows()
        {
            Action act = () => S3InitilisationExtensions.ConfigureS3((IServiceCollection) null, _mockConfiguration.Object);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConfigureS3TestNullConfigurationThrows()
        {
            Action act = () => S3InitilisationExtensions.ConfigureS3(new ServiceCollection(), null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(S3Url)]
        public void ConfigureS3TestRegistersServices(string path)
        {
            ConfigureConfig(_mockConfiguration, path);

            var services = new ServiceCollection();
            services.ConfigureS3(_mockConfiguration.Object);

            _mockConfiguration.Verify(x => x.GetSection(ConfigKey), Times.Once);

            var serviceProvider = services.BuildServiceProvider();
            var s3Client = serviceProvider.GetService<IAmazonS3>();
            s3Client.Should().NotBeNull();
            s3Client.Config.ServiceURL.Should().Be(string.IsNullOrEmpty(path)? null : path);
        }
    }
}
