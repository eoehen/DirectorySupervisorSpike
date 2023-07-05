using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oehen.arguard;

namespace DirectorySupervisorSpike.App.filesystem
{
    internal class DirectoryParser : IDirectoryParser
    {
        private readonly ILogger<DirectoryParser> logger;
        private readonly IOptions<DirectorySupervisorOptions> directorySupervisorOptions;

        public DirectoryParser(
            ILogger<DirectoryParser> logger,
            IOptions<DirectorySupervisorOptions> directorySupervisorOptions)
        {
            this.logger = logger;
            this.directorySupervisorOptions = directorySupervisorOptions;
        }

        public List<string> ParseDirectoryFiles(string sstDirectory, DirectorySupervisorDirOptions directorySupervisorDirOptions)
        {
            sstDirectory.ThrowIfIsNullOrWhiteSpace(nameof(sstDirectory));
            directorySupervisorDirOptions.ThrowIfNull(nameof(directorySupervisorDirOptions));

            var globalOptions = directorySupervisorOptions?.Value;
            globalOptions.ThrowIfNull(nameof(globalOptions));

            if (!Directory.Exists(sstDirectory))
            {
                logger.LogWarning($"Directory not found '{sstDirectory}'");
                return new List<string>();
            }

            Matcher matcher = new();

            AppendIncludePattern(matcher, directorySupervisorDirOptions, globalOptions);
            AppendExcludePattern(matcher, directorySupervisorDirOptions, globalOptions);

            logger.LogDebug($"Scan directory path: '{sstDirectory}'");
            var results = matcher.GetResultsInFullPath(sstDirectory).ToList();
            logger.LogDebug($"found {results.Count} file(s).");

            return results;
        }

        private void AppendExcludePattern(Matcher matcher, DirectorySupervisorDirOptions directorySupervisorDirOptions, DirectorySupervisorOptions? globalOptions)
        {
            if (globalOptions != null)
            {
                matcher.AddExcludePatterns(globalOptions.GlobalExcludePatterns);
            }
            matcher.AddExcludePatterns(directorySupervisorDirOptions.ExcludePatterns);
            foreach (var excludePattern in directorySupervisorDirOptions.ExcludePatterns)
            {
                logger.LogDebug($"exclude search pattern '{excludePattern}'");
            }
        }

        private void AppendIncludePattern(Matcher matcher, DirectorySupervisorDirOptions directorySupervisorDirOptions, DirectorySupervisorOptions? globalOptions)
        {
            if (globalOptions != null)
            {
                matcher.AddIncludePatterns(globalOptions.GlobalIncludePatterns);
            }
            matcher.AddIncludePatterns(directorySupervisorDirOptions.IncludePatterns);
            foreach (var includePattern in directorySupervisorDirOptions.IncludePatterns)
            {
                logger.LogDebug($"include search pattern '{includePattern}'");
            }
        }
    }
}
