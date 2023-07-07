using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.hashData
{
    internal class DirectoryHashDataFileNameBuilder : IDirectoryHashDataFileNameBuilder
    {
        const string directorySupervisorDataFileNameFormat = "directorySupervisorData-{0}.json";

        public string Build(DirectorySupervisorDirOptions directorySupervisorDirOptions)
        {
            var baseDirectory = directorySupervisorDirOptions.Path;

            var directoryInfo = new DirectoryInfo(baseDirectory);
            var fileName = string.Format(directorySupervisorDataFileNameFormat,
                                          directoryInfo.Name);
            return fileName;
        }
    }
}
