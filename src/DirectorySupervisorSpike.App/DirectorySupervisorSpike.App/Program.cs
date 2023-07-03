using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectorySupervisorSpike.App
{
    internal sealed class Program
    {
        private Program()
        {
        }

        static void Main(string[] args)
        {
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
                });
        }
    }
}