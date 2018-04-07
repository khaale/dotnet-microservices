using System;
using System.IO;
using System.Threading;
using CustomerService.Core.Api;
using CustomerService.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Winton.Extensions.Configuration.Consul;

namespace CustomerService
{
    public class Startup
    {
        private readonly CancellationTokenSource _consulCts = new CancellationTokenSource();
        
        public Startup(IConfiguration configuration)
        {
            var noConsul = configuration.GetValue<bool>("NO_CONSUL", false);

            Configuration = noConsul 
                ? configuration 
                : BuildDynamicConfiguration(configuration);
        }

        private IConfigurationRoot BuildDynamicConfiguration(IConfiguration configuration)
        {
            // prepare configuration that will be used for serving web requests
            var builder = new ConfigurationBuilder()
                // apply static config values from initial configuration
                .AddInMemoryCollection(configuration.AsEnumerable())
                // apply dynamic configuration from Consul
                // currently only works with a single Consul KV value, which must contain all the configuration as json
                .AddConsul(
                    "customer-service",
                    _consulCts.Token,
                    options =>
                    {
                        options.ConsulConfigurationOptions = cco => { cco.Address = new Uri("http://consul:8500"); };
                        options.Optional = true;
                        options.ReloadOnChange = true;
                        options.OnLoadException = exceptionContext =>
                        {
                            Log.Warning("Error loading configuration from Consul: {Message}",
                                exceptionContext.Exception.Message);
                            exceptionContext.Ignore = true;
                        };
                    });

            return builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<CustomerOptions>(Configuration.GetSection("customerOptions"));

            services.AddMvcCore().AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddMvc();
            services.AddApiVersioning(o => o.ReportApiVersions = true);
            services.AddSwaggerGen(options =>
            {
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                // add a swagger document for each discovered API version
                // note: you might choose to skip or document deprecated API versions differently
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }

                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "CustomerService.xml");
                options.IncludeXmlComments(filePath);
            });
        }
        
        static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info()
            {
                Title = $"Sample API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime, IApiVersionDescriptionProvider provider)
        {
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                Log.Information("Cancelling Consul requests and watches..");
                _consulCts.Cancel();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }
    }
}
