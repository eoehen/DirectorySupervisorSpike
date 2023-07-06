using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using oehen.arguard;

namespace DirectorySupervisorSpike.App.hashData
{
    internal class HashDataManager : IHashDataManager
    {
        const string directorySupervisorDataFileName = "directorySupervisorData.json";

        private static readonly JsonSerializerSettings jsonSerializerSettings
            = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
        private readonly IOptions<DirectorySupervisorOptions> directorySupervisorOptions;
        private readonly ILogger<HashDataManager> logger;

        public HashDataManager(
            IOptions<DirectorySupervisorOptions> directorySupervisorOptions,
            ILogger<HashDataManager> logger)
        {
            this.directorySupervisorOptions = directorySupervisorOptions;
            this.logger = logger;
        }

        public async Task<DirectorySupervisorData> LoadJsonFileAsync(string baseDirectory, CancellationToken cancellationToken = default)
        {
            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, directorySupervisorDataFileName);

            if (!File.Exists(directorySupervisorDataFullPath))
            {
                await WriteJsonFileAsync(baseDirectory, new DirectorySupervisorData(), cancellationToken);
            }

            var directorySupervisorData = ReadJsonFile(directorySupervisorDataFullPath);

            SyncDirectories(baseDirectory, directorySupervisorData);
            await WriteJsonFileAsync(baseDirectory, directorySupervisorData, cancellationToken);

            return directorySupervisorData;
        }

        private static DirectorySupervisorData ReadJsonFile(string directorySupervisorDataFullPath)
        {
            directorySupervisorDataFullPath.ThrowIfIsNullOrWhiteSpace(nameof(directorySupervisorDataFullPath));

            using var streamReader = new StreamReader(directorySupervisorDataFullPath);
            var json = streamReader.ReadToEnd();
            return JsonConvert.DeserializeObject<DirectorySupervisorData>(json) ?? new DirectorySupervisorData();
        }

        public async Task WriteJsonFileAsync(string baseDirectory, DirectorySupervisorData directorySupervisorData, CancellationToken cancellationToken = default)
        {
            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, directorySupervisorDataFileName);

            var jsonString = JsonConvert.SerializeObject(directorySupervisorData, jsonSerializerSettings);
            await File.WriteAllTextAsync(directorySupervisorDataFullPath, jsonString, cancellationToken);
        }

        public void EvaluateAndUpdateDirectoryHash(DirectoryHashData directoryHashData, string sstDirectoryHash)
        {
            directoryHashData.ThrowIfNull(nameof(directoryHashData));
            sstDirectoryHash.ThrowIfNull(nameof(sstDirectoryHash));

            directoryHashData.LastHashCheck = DateTime.Now;

            if (directoryHashData.LastDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(sstDirectoryHash))
            {
                directoryHashData.LastDirectoryHash = sstDirectoryHash;
                directoryHashData.LastDirectoryHashDifferentSince = DateTime.Now;

                logger.LogWarning($"different directory hash detected {directoryHashData.DirectoryPath}");
            }

            if (directoryHashData.CurrentDirectoryHash == null || !directoryHashData.LastDirectoryHash.Equals(directoryHashData.CurrentDirectoryHash))
            {
                var options = directorySupervisorOptions?.Value;
                if (options == null) { return; }

                // No LastDirectoryHash change for n min
                if (directoryHashData.LastDirectoryHashDifferentSince < DateTime.Now.AddMinutes(options.ChangeDetectionDelayMinutes*-1))
                {
                    directoryHashData.CurrentDirectoryHash = directoryHashData.LastDirectoryHash;
                    directoryHashData.ImportPending = true;

                    // TODO enqueue import!!
                    logger.LogWarning($"--> enque import {directoryHashData.DirectoryPath}");

                    directoryHashData.ImportPending = false;
                }
            }
        }

        private void SyncDirectories(string baseDirectory, DirectorySupervisorData directorySupervisorData)
        {
            var sstDirectories = Directory.GetDirectories(baseDirectory).Where(d => Directory.Exists(d)).ToArray();
            AppendMissingDirectories(directorySupervisorData, sstDirectories);
            RemoveNotExistingDirectories(directorySupervisorData, sstDirectories);
        }

        private static void RemoveNotExistingDirectories(DirectorySupervisorData directorySupervisorData, string[] sstDirectories)
        {
            directorySupervisorData.DirectoryHashDatas
                .RemoveAll(dh => 
                directorySupervisorData.DirectoryHashDatas
                    .Select(d => d.DirectoryPath).Except(sstDirectories).ToList().Contains(dh.DirectoryPath));
        }

        private static void AppendMissingDirectories(DirectorySupervisorData directorySupervisorData, string[] sstDirectories)
        {
            directorySupervisorData.DirectoryHashDatas.AddRange(
                sstDirectories.Except(directorySupervisorData.DirectoryHashDatas.Select(d => d.DirectoryPath))
                .Select(md => new DirectoryHashData(md)));
        }
    }
}
