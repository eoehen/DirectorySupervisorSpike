using Newtonsoft.Json;
using oehen.arguard;

namespace DirectorySupervisorSpike.App.hashData
{
    internal class HashDataManager : IHashDataManager
    {
        const string directorySupervisorDataFileName = "directorySupervisorData.json";

        private static readonly JsonSerializerSettings jsonSerializerSettings
            = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public async Task<DirectorySupervisorData> LoadJsonFileAsync(string baseDirectory)
        {
            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, directorySupervisorDataFileName);

            if (!File.Exists(directorySupervisorDataFullPath))
            {
                await WriteJsonFileAsync(baseDirectory, new DirectorySupervisorData());
            }

            var directorySupervisorData = ReadJsonFile(directorySupervisorDataFullPath);

            SyncDirectories(baseDirectory, directorySupervisorData);
            await WriteJsonFileAsync(baseDirectory, directorySupervisorData);

            return directorySupervisorData;
        }

        private static DirectorySupervisorData ReadJsonFile(string directorySupervisorDataFullPath)
        {
            directorySupervisorDataFullPath.ThrowIfIsNullOrWhiteSpace(nameof(directorySupervisorDataFullPath));

            using var streamReader = new StreamReader(directorySupervisorDataFullPath);
            var json = streamReader.ReadToEnd();
            return JsonConvert.DeserializeObject<DirectorySupervisorData>(json) ?? new DirectorySupervisorData();
        }

        public async Task WriteJsonFileAsync(string baseDirectory, DirectorySupervisorData directorySupervisorData)
        {
            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, directorySupervisorDataFileName);

            var jsonString = JsonConvert.SerializeObject(directorySupervisorData, jsonSerializerSettings);
            await File.WriteAllTextAsync(directorySupervisorDataFullPath, jsonString);
        }

        private void SyncDirectories(string baseDirectory, DirectorySupervisorData directorySupervisorData)
        {
            var sstDirectories = Directory.GetDirectories(baseDirectory).Where(d => Directory.Exists(d)).ToArray();
            AppendMissingDirectories(directorySupervisorData, sstDirectories);
            RemoveNotExistingDirectories(directorySupervisorData, sstDirectories);
        }

        private static void RemoveNotExistingDirectories(DirectorySupervisorData directorySupervisorData, string[] sstDirectories)
        {
            directorySupervisorData.DirectoryHashData
                .RemoveAll(dh => 
                directorySupervisorData.DirectoryHashData
                    .Select(d => d.DirectoryName).Except(sstDirectories).ToList().Contains(dh.DirectoryName));
        }

        private static void AppendMissingDirectories(DirectorySupervisorData directorySupervisorData, string[] sstDirectories)
        {
            directorySupervisorData.DirectoryHashData.AddRange(
                sstDirectories.Except(directorySupervisorData.DirectoryHashData.Select(d => d.DirectoryName))
                .Select(md => new DirectoryHashData(md)));
        }
    }
}
