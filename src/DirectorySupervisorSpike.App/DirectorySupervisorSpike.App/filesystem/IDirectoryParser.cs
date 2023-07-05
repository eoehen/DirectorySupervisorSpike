using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.filesystem
{
    internal interface IDirectoryParser
    {
        List<string> ParseDirectoryFiles(string sstDirectory, DirectorySupervisorDirOptions directorySupervisorDirOptions);
    }
}