using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IDirectoryHashDataFilePathBuilder
    {
        string Build(DirectorySupervisorDirOptions directorySupervisorDirOptions);
    }
}