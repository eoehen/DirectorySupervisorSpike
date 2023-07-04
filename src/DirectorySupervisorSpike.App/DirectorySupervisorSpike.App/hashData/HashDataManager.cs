using Newtonsoft.Json;

namespace DirectorySupervisorSpike.App.hashData
{
    internal class HashDataManager : IHashDataManager
    {
        const string directorySupervisorDataFileName = "directorySupervisorData.json";

        private static readonly JsonSerializerSettings jsonSerializerSettings
            = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        public DirectorySupervisorData LoadJsonFile(string baseDirectory)
        {
            var directorySupervisorDataFullPath = 
                Path.Combine(baseDirectory, directorySupervisorDataFileName);

            if (!File.Exists(directorySupervisorDataFullPath))
            {
                WriteJsonFile(baseDirectory, new DirectorySupervisorData());
            }

            using var streamReader = new StreamReader(directorySupervisorDataFullPath);
            var json = streamReader.ReadToEnd();
            return JsonConvert.DeserializeObject<DirectorySupervisorData>(json) ?? new DirectorySupervisorData();
        }

        public void WriteJsonFile(string baseDirectory, DirectorySupervisorData directorySupervisorData)
        {
            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, directorySupervisorDataFileName);
            var jsonString = JsonConvert.SerializeObject(directorySupervisorData, jsonSerializerSettings);
            File.WriteAllText(directorySupervisorDataFullPath, jsonString);
        }

    }
}
