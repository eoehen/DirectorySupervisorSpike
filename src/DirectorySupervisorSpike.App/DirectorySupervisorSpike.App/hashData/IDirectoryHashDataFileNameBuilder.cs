using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IDirectoryHashDataFileNameBuilder
    {
        string Build(DirectorySupervisorDirOptions directorySupervisorDirOptions);
    }
}