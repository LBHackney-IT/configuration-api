using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ConfigurationApi.V1.Domain;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ConfigurationApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/configuration")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ConfigurationController : Controller
    {
        private readonly IAmazonS3 _client;

        public ConfigurationController(IAmazonS3 client)
        {
            _client = client;
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string[] types)
        {
            try
            {
                var listOfConfigurations = new List<ApiConfiguration>();
                var bucketName = Environment.GetEnvironmentVariable("CONFIGURATION_S3_BUCKETNAME");

                foreach (string type in types)
                {
                    GetObjectRequest request = new GetObjectRequest {BucketName = bucketName, Key = type};

                    using (GetObjectResponse response = await _client.GetObjectAsync(request))
                    using (Stream responseStream = response.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        string title =
                            response.Metadata[
                                "x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                        string contentType = response.Headers["Content-Type"];
                        Console.WriteLine("Object metadata, Title: {0}", title);
                        Console.WriteLine("Content type: {0}", contentType);

                        listOfConfigurations.Add(JsonConvert.DeserializeObject<ApiConfiguration>(reader.ReadToEnd()));
                    }
                }

                return Ok(listOfConfigurations);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
