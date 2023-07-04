using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;

namespace DirectorySupervisorSpike.App
{
    internal class Worker
    {
        private readonly ILogger<Worker> logger;
        private readonly IOptions<DirectorySupervisorOptions> directorySupervisorOptions;

        public Worker(
            ILogger<Worker> logger,
            IOptions<DirectorySupervisorOptions> directorySupervisorOptions)
        {
            this.logger = logger;
            this.directorySupervisorOptions = directorySupervisorOptions;
        }

        public void Execute()
        {
            logger.LogInformation("==============================================");
            logger.LogInformation("DirectorySupervisor configurations...");
            logger.LogInformation("==============================================");

            // var keyValuePairs = configuration.AsEnumerable().ToList();
            //foreach (var pair in keyValuePairs)
            //{
            //    Console.WriteLine($"{pair.Key} - {pair.Value}");
            //}
            //Console.WriteLine("==============================================");
            //Console.ResetColor();

            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            var patternMatchingResults = new List<string>();

            Matcher matcher = new();

            var directories = options.Directories
                .Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory.Path))
                {
                    logger.LogWarning($"Base directory path: '{directory.Path}' not found.");
                    continue;
                }

                logger.LogInformation($"Base directory path: '{directory.Path}'");

                /* iterate first level of directories */

                string[] sstDirectories = Directory.GetDirectories(directory.Path);

                foreach (string sstDirectory in sstDirectories)
                {
                    logger.LogInformation($"--------------------------------------------------");
                    logger.LogInformation($"use sst directory: '{sstDirectory}'");

                    logger.LogDebug("include pattern:");
                    matcher.AddIncludePatterns(options.GlobalIncludePatterns);
                    matcher.AddIncludePatterns(directory.IncludePatterns);
                    foreach (var includePattern in directory.IncludePatterns)
                    {
                        logger.LogDebug($" - {includePattern}");
                    }

                    logger.LogDebug("exclude pattern:");
                    matcher.AddExcludePatterns(options.GlobalExcludePatterns);
                    matcher.AddExcludePatterns(directory.ExcludePatterns);
                    foreach (var excludePattern in directory.ExcludePatterns)
                    {
                        logger.LogDebug($" - {excludePattern}");
                    }

                    var results = matcher.GetResultsInFullPath(sstDirectory);
                    logger.LogInformation($"found {results.Count()} file(s).");

                    patternMatchingResults.AddRange(results);

                }
            }

            logger.LogInformation("");
            logger.LogInformation($"Found Paths: {patternMatchingResults.Count} file(s).");
            foreach (var patternMatchingResultFile in patternMatchingResults)
            {
                logger.LogDebug($" - {patternMatchingResultFile}");
            }
        }
    }
}