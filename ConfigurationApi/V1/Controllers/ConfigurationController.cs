using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Amazon.S3.Model;
using Hackney.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConfigurationApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/configuration")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ConfigurationController : Controller
    {
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("Get")]
        public IActionResult Get()
        {
            using (var reader = new StreamReader("JsonFile.txt"))
            {
                var result = reader.ReadToEnd();

                return Content(result, "application/json");

            }
        }
    }
}
