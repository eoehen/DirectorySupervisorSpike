using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.hashData;

namespace DirectorySupervisorSpike.App.filesystem
{
    internal interface ISstDirectoryHashCalculator
    {
        Task<string> CalcDirectoryHashAsync(DirectorySupervisorDirOptions directoryOptions, DirectoryHashData directoryHashData);
    }
}