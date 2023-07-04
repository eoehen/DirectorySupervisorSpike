using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DirectorySupervisorSpike.App
{
    internal sealed class Program
    {
        private Program()
        {
        }

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()  // initiate the logger configuration
                .ReadFrom.AppSettings()             // Connect serilog to our configuration folder
                .Enrich.FromLogContext()            // Adds more information to our logs from built in Serilog 
                .WriteTo.Console()                  // Decide where the logs are going to be shown
                .CreateLogger();                    // Initialise the logger

            var host = CreateDefaultBuilder().Build();
            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var workerInstance = provider.GetRequiredService<Worker>();
            workerInstance.Execute();
            host.Run();
        }

        static IHostBuilder CreateDefaultBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(configure =>
                {
                    configure.AddJsonFile("appsettings.json");
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Worker>();
                    services.Configure<DirectorySupervisorOptions>(context.Configuration.GetSection("DirectorySupervisor"));
                })
                .UseSerilog()
                ;
        }
    }
}