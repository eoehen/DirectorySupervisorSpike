using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.hashData;
using DirectorySupervisorSpike.App.performance;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

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
            logger.LogInformation(" Start DirectorySupervisor");
            logger.LogInformation("==============================================");

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync())
            {
                await SuperviseDirectoryAsync();
            }
        }

        private async Task SuperviseDirectoryAsync()
        {
            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            var directories = options.Directories
                .Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory.Path))
                {
                    logger.LogWarning($"Base directory path: '{directory.Path}' not found.");
                    continue;
                }

                logger.LogDebug($"Scan directory path: '{directory.Path}'");

                var directoryTimepiece = Timepiece.StartNew();

                var directorySupervisorData = await this.hashDataManager.LoadJsonFileAsync(directory.Path)
                    .ConfigureAwait(false);

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
                        logger.LogWarning($"Import for '{sstDirectory}' is pending.");
                        continue;
                    }

                    var sstDirTimepiece = Timepiece.StartNew();

                    Matcher matcher = new();

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
                    logger.LogDebug($"found {results.Count} file(s).");

                    var sstDirectoryHash = await hashBuilder.BuildAsync(sstDirectory, results).ConfigureAwait(false);

                    logger.LogDebug($"directory hash '{sstDirectoryHash}'");

                    if (directoryHashData.LastDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(sstDirectoryHash))
                    {
                        directoryHashData.LastDirectoryHash = sstDirectoryHash;
                        directoryHashData.LastDirectoryHashDifferentSince = DateTime.Now;

                        logger.LogWarning($"different directory hash detected {directoryHashData.DirectoryName}");
                    }

                    if (directoryHashData.CurrentDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(directoryHashData.CurrentDirectoryHash))
                    {
                        // No LastDirectoryHash change for 1 min
                        if (directoryHashData.LastDirectoryHashDifferentSince < DateTime.Now.AddMinutes(-1))
                        {
                            directoryHashData.CurrentDirectoryHash = directoryHashData.LastDirectoryHash;
                            directoryHashData.ImportPending = true;

                            logger.LogError($"--> enque import {directoryHashData.DirectoryName}");

                            directoryHashData.ImportPending = false;

                            // TODO enqueue import!!
                        }
                    }

                    directoryHashData.LastHashCheck = DateTime.Now;

                    logger.LogDebug($"SST directory path: '{sstDirectory}' elapsed {sstDirTimepiece.GetElapsedTime()}");
                }

                await this.hashDataManager.WriteJsonFileAsync(directory.Path, directorySupervisorData).ConfigureAwait(false);

                logger.LogInformation($"Directory path: '{directory.Path}' elapsed {directoryTimepiece.GetElapsedTime()}");
            }
        }
    }
}