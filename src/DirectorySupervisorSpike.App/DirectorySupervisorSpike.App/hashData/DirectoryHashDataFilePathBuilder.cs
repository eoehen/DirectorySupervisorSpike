using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.hashData
{
    internal class DirectoryHashDataFilePathBuilder : IDirectoryHashDataFilePathBuilder
    {
        private readonly IDirectoryHashDataFileNameBuilder directoryHashDataFileNameBuilder;

        public DirectoryHashDataFilePathBuilder(IDirectoryHashDataFileNameBuilder directoryHashDataFileNameBuilder)
        {
            this.directoryHashDataFileNameBuilder = directoryHashDataFileNameBuilder;
        }

        public string Build(DirectorySupervisorDirOptions directorySupervisorDirOptions)
        {
            var fileName =
                this.directoryHashDataFileNameBuilder.Build(directorySupervisorDirOptions);

            var directoryInfo = new DirectoryInfo(directorySupervisorDirOptions.Path);
            var baseDirectory = directoryInfo.FullName;

            if (!directorySupervisorDirOptions.UseSubDirectories)
            {
                baseDirectory = directoryInfo.Parent.FullName;
            }

            var directorySupervisorDataFullPath =
                Path.Combine(baseDirectory, fileName);

            return directorySupervisorDataFullPath;
        }
    }
}
