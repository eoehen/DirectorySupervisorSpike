using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.hashData;
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
        private readonly IHashDataManager hashDataManager;
        private readonly IDirectoryHashBuilder hashBuilder;

        public Worker(
            ILogger<Worker> logger,
            IOptions<DirectorySupervisorOptions> directorySupervisorOptions,
            IHashDataManager hashDataManager,
            IDirectoryHashBuilder hashBuilder)
        {
            this.logger = logger;
            this.directorySupervisorOptions = directorySupervisorOptions;
            this.hashDataManager = hashDataManager;
            this.hashBuilder = hashBuilder;
        }

        public async Task ExecuteAsync()
        {
            logger.LogInformation("==============================================");
            logger.LogInformation("DirectorySupervisor configurations...");
            logger.LogInformation("==============================================");

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync())
            {
                SuperviseDirectory();
            }
        }

        private void SuperviseDirectory()
        {
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

                var directorySupervisorData = this.hashDataManager.LoadJsonFile(directory.Path);

                /* iterate first level of directories */

                string[] sstDirectories = Directory.GetDirectories(directory.Path);

                foreach (string sstDirectory in sstDirectories)
                {
                    var directoryHashData =
                        directorySupervisorData.DirectoryHashData
                        .SingleOrDefault(d => d.DirectoryName == sstDirectory);

                    if (directoryHashData == null)
                    {
                        directoryHashData = new DirectoryHashData();
                        directoryHashData.DirectoryName = sstDirectory;
                        directorySupervisorData.DirectoryHashData.Add(directoryHashData);
                    }

                    if (directoryHashData.ImportPending)
                    {
                        logger.LogInformation($"Import for '{sstDirectory}' is pending.");
                        continue;
                    }

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

                    var results = matcher.GetResultsInFullPath(sstDirectory).ToList();
                    logger.LogInformation($"found {results.Count} file(s).");

                    var sstDirectoryHash = this.hashBuilder.Build(sstDirectory, results);
                    logger.LogInformation($"directory hash '{sstDirectoryHash}'");

                    if (directoryHashData.LastDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(sstDirectoryHash))
                    {
                        directoryHashData.LastDirectoryHash = sstDirectoryHash;
                        directoryHashData.LastDirectoryHashDifferentSince = DateTime.Now;
                    }

                    if (directoryHashData.CurrentDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(directoryHashData.CurrentDirectoryHash))
                    {
                        // No LastDirectoryHash change for 1 min
                        if (directoryHashData.LastDirectoryHashDifferentSince < DateTime.Now.AddMinutes(-1))
                        {
                            directoryHashData.CurrentDirectoryHash = directoryHashData.LastDirectoryHash;
                            directoryHashData.ImportPending = true;
                        }
                    }

                    directoryHashData.LastHashCheck = DateTime.Now;

                    patternMatchingResults.AddRange(results);
                }

                this.hashDataManager.WriteJsonFile(directory.Path, directorySupervisorData);
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