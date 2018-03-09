using System;
using System.IO;
using System.Threading;
using CustomerService.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        // TODO: Try avoid public exposing
        private readonly CancellationTokenSource ConsulCts = new CancellationTokenSource();
        
        public Startup(IConfiguration configuration)
        {
            // prepare configuration that will be used for serving web requests
            var builder = new ConfigurationBuilder()
                // apply static config values from initial configuration
                .AddInMemoryCollection(configuration.AsEnumerable())
                // apply dynamic configuration from Consul
                // currently only works with a single Consul KV value, which must contain all the configuration as json
                .AddConsul(
                    "customer-service",
                    ConsulCts.Token,
                    options =>
                    {
                        options.ConsulConfigurationOptions = cco =>
                        {
                            cco.Address = new Uri("http://consul:8500");
                        };
                        options.Optional = true;
                        options.ReloadOnChange = true;
                        options.OnLoadException = exceptionContext =>
                        {
                            Log.Warning(exceptionContext.Exception, "Error loading configuration from Consul");
                            exceptionContext.Ignore = true;
                        };
                    });

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<CustomerOptions>(Configuration.GetSection("customerOptions"));

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "My API - V1",
                        Version = "v1"
                    }
                );

                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "CustomerService.xml");
                c.IncludeXmlComments(filePath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                Log.Information("Cancelling Consul requests and watches..");
                ConsulCts.Cancel();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
