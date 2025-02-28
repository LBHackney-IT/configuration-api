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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ConfigurationApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public IConfiguration Configuration { get; }
        private static List<ApiVersionDescription> _apiVersions { get; set; }
        private const string ApiName = "Configuration-Api";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services
                .AddMvc()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
            });

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Your Hackney API Key",
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Token" }
                        },
                        new List<string>()
                    }
                });

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in _apiVersions)
                {
                    var version = $"v{apiVersion.ApiVersion.ToString()}";
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{ApiName}-api {version}",
                        Version = version,
                        Description = $"{ApiName} version {version}. Please check older versions for depreciated endpoints."
                    });
                }

                c.CustomSchemaIds(x => x.FullName);
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            AWSXRayRecorder.InitializeInstance(Configuration);
            AWSXRayRecorder.RegisterLogger(LoggingOptions.SystemDiagnostics);

            services.ConfigureLambdaLogging(Configuration);
            services.AddLogCallAspect();
            services.AddTokenFactory();
            services.ConfigureS3(Configuration);

            RegisterGateways(services);
            RegisterUseCases(services);
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.TryAddScoped<IConfigurationGateway, S3ConfigurationGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.TryAddScoped<IConfigurationUseCase, ConfigurationUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("x-correlation-id"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCorrelationId();
            app.UseLoggingScope();
            app.UseCustomExceptionHandler(logger);
            app.UseXRay("configuration-api");
            app.UseLogCall();


            //Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            _apiVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in _apiVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName}-api {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });
            app.UseSwagger();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
