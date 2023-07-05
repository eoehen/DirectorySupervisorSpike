using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.filesystem;
using DirectorySupervisorSpike.App.hashData;
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

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()  // initiate the logger configuration
                .ReadFrom.AppSettings()             // Connect serilog to our configuration folder
                .Enrich.FromLogContext()            // Adds more information to our logs from built in Serilog 
                .WriteTo.Console()                  // Decide where the logs are going to be shown
                .CreateLogger();                    // Initialise the logger

            var host = CreateDefaultBuilder().Build();
            await using var serviceScope = host.Services.CreateAsyncScope();
            var provider = serviceScope.ServiceProvider;
            var workerInstance = provider.GetRequiredService<Worker>();
            await workerInstance.ExecuteAsync();
            await host.RunAsync();
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
                    services.Configure<DirectorySupervisorOptions>(context.Configuration.GetSection("DirectorySupervisor"));
                    services.AddSingleton<Worker>();
                    services.AddSingleton<IDirectoryHashBuilder, DirectoryHashBuilder>();
                    services.AddSingleton<IHashDataManager, HashDataManager>();
                    services.AddSingleton<IDirectoryParser, DirectoryParser>();
                })
                .UseSerilog()
                ;
        }
    }
}