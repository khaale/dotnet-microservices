using System;
using System.IO;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Winton.Extensions.Configuration.Consul;

namespace CustomerService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                // prepare configuration that will be used during startup and initialization
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .AddCommandLine(cs => cs.Args = args);
                var initialConfiguration = builder.Build();

                BuildWebHost(initialConfiguration).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(IConfiguration configuration) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseMetrics()
                .UseConfiguration(configuration)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
