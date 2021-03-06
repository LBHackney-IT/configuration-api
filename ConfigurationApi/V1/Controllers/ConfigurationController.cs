using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ConfigurationApi.V1.Domain;
using ConfigurationApi.V1.UseCase;
using Hackney.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConfigurationApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/configuration")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationUseCase _configurationUseCase;

        public ConfigurationController(IConfigurationUseCase configurationUseCase)
        {
            _configurationUseCase = configurationUseCase;
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> Get([FromQuery] string[] types)
        {
            var listOfConfigurations = await _configurationUseCase.Get(types);

            return Ok(listOfConfigurations);
        }
    }
}
