using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ConfigurationApi.V1.Domain;
using Newtonsoft.Json;

namespace ConfigurationApi.V1.Gateway
{
    public class S3ConfigurationGateway : IConfigurationGateway
    {
        private readonly IAmazonS3 _amazonS3Client;

        public S3ConfigurationGateway(IAmazonS3 amazonS3Client)
        {
            _amazonS3Client = amazonS3Client;
        }

        public async Task<ApiConfiguration> Get(string type)
        {
            var bucketName = Environment.GetEnvironmentVariable("CONFIGURATION_S3_BUCKETNAME");

            GetObjectRequest request = new GetObjectRequest { BucketName = bucketName, Key = type };

            using (GetObjectResponse response = await _amazonS3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                return JsonConvert.DeserializeObject<ApiConfiguration>(reader.ReadToEnd());
            }
        }
    }
}
