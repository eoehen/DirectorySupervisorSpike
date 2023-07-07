using DirectorySupervisorSpike.App.configuration;

namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IHashDataManager
    {
        Task<DirectorySupervisorData> LoadJsonFileAsync(DirectorySupervisorDirOptions directorySupervisorDirOptions, CancellationToken cancellationToken = default);
        Task WriteJsonFileAsync(DirectorySupervisorDirOptions directorySupervisorDirOptions, DirectorySupervisorData directorySupervisorData, CancellationToken cancellationToken = default);
        void EvaluateAndUpdateDirectoryHash(DirectoryHashData directoryHashData, string sstDirectoryHash);
    }
}