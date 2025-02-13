using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using ConfigurationApi.V1.Gateway;
using ConfigurationApi.V1.Infrastructure;
using ConfigurationApi.V1.UseCase;
using ConfigurationApi.Versioning;
using FluentValidation.AspNetCore;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Middleware.CorrelationId;
using Hackney.Core.Middleware.Exception;
using Hackney.Core.Middleware.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// AWS X-Ray setup
AWSSDKHandler.RegisterXRayForAllServices();
AWSXRayRecorder.InitializeInstance(configuration);
AWSXRayRecorder.RegisterLogger(LoggingOptions.SystemDiagnostics);

// Add services to the container
var services = builder.Services;

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("x-correlation-id"));
});

services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
});

services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

// Add additional services
services.ConfigureLambdaLogging(configuration);
services.AddLogCallAspect();
services.AddTokenFactory();
services.ConfigureS3(configuration);

// Dependency injection for gateways and use cases
services.AddScoped<IConfigurationGateway, S3ConfigurationGateway>();
services.AddScoped<IConfigurationUseCase, ConfigurationUseCase>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseCorrelationId();
app.UseLoggingScope();
app.UseCustomExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());
app.UseXRay("configuration-api");
app.UseLogCall();
app.UseCors();

// Enable Swagger if still needed
var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
var apiVersions = apiVersionProvider.ApiVersionDescriptions.ToList();

if (apiVersions.Count > 0)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var apiVersion in apiVersions)
        {
            c.SwaggerEndpoint($"{apiVersion.GetFormattedApiVersion()}/swagger.json",
                $"Configuration-Api {apiVersion.GetFormattedApiVersion()}");
        }
    });
}

app.MapControllers();
app.Run();
