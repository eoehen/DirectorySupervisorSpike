﻿using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.crypto;
using DirectorySupervisorSpike.App.filesystem;
using DirectorySupervisorSpike.App.hashData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using oehen.arguard;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace DirectorySupervisorSpike.App
{
    internal sealed class Program
    {
        private Program()
        {
        }

        static async Task Main(string[] args)
        {
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
                    services.AddSingleton<ISstDirectoryHashCalculator, SstDirectoryHashCalculator>();
                    services.AddSingleton<IDirectoryHashDataFileNameBuilder, DirectoryHashDataFileNameBuilder>();
                    services.AddSingleton<IDirectoryHashDataFilePathBuilder, DirectoryHashDataFilePathBuilder>();
                })
                .UseSerilog((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File("directorySupervisor.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, rollingInterval: RollingInterval.Day)
                        .WriteTo.File("directorySupervisor-warn.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
                        .ReadFrom.Configuration(hostingContext.Configuration);
                })
                ;
        }
    }
}