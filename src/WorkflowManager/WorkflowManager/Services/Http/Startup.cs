/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Monai.Deploy.WorkflowManager.Authentication.Extensions;
using Monai.Deploy.WorkflowManager.Logging.Attributes;
using Monai.Deploy.WorkflowManager.Shared;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.WorkflowManager.Services.Http
{
    /// <summary>
    /// Http Api Endpoint Startup Class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configurations.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

#pragma warning disable SA1600 // Elements should be documented
        public IConfiguration Configuration { get; }
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// Configure Services.
        /// </summary>
        /// <param name="services">Services Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddHttpContextAccessor();
            services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // Add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // NOTE: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // NOTE: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddControllers(options => options.Filters.Add(typeof(LogActionFilterAttribute))).AddNewtonsoftJson(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MONAI Workflow Manager", Version = "v1" });
                c.DescribeAllParametersInCamelCase();
            });

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Startup>>();

            services.AddMonaiAuthentication(Configuration, logger);

            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestQuery |
                                        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState);

                    var result = new BadRequestObjectResult(problemDetails);

                    result.ContentTypes.Add("application/problem+json");
                    result.ContentTypes.Add("application/problem+xml");

                    return result;
                };
            });

            services.AddHealthChecks()
                .AddCheck<MonaiHealthCheck>("Workflow Manager Services")
                .AddMongoDb(mongodbConnectionString: Configuration["WorkloadManagerDatabase:ConnectionString"], mongoDatabaseName: Configuration["WorkloadManagerDatabase:DatabaseName"]);
        }

        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Web Host Environment.</param>
#pragma warning disable SA1204 // Static elements should appear before instance elements
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#pragma warning restore SA1204 // Static elements should appear before instance elements
        {
            if (env.IsProduction() is false)
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "aspnet6 v1"));
            }

            app.UseRouting();
            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(c => new
                        {
                            check = c.Key,
                            result = c.Value.Status.ToString(),
                        }),
                    });

                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                },
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpointAuthorizationMiddleware();
            app.UseHttpLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
