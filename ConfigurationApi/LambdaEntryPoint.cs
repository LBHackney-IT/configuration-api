using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;
using Hackney.Core.Middleware.CorrelationId;
using Hackney.Core.Middleware.Exception;
using Hackney.Core.Middleware.Logging;

namespace ConfigurationApi
{
    public class LambdaEntryPoint : APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Register services (same as in Program.cs)
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .WithExposedHeaders("x-correlation-id"));
                });

                services.AddControllers();

                services.AddApiVersioning(o =>
                {
                    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader();
                });

                services.AddSingleton<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider, Microsoft.AspNetCore.Mvc.ApiExplorer.DefaultApiVersionDescriptionProvider>();
            });

            builder.Configure(app =>
            {
                var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                var logger = app.ApplicationServices.GetRequiredService<ILogger<LambdaEntryPoint>>();

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
                app.UseCors();

                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
