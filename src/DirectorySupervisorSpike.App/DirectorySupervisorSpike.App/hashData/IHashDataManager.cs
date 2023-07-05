namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IHashDataManager
    {
        Task<DirectorySupervisorData> LoadJsonFileAsync(string baseDirectory, CancellationToken cancellationToken = default);
        Task WriteJsonFileAsync(string baseDirectory, DirectorySupervisorData directorySupervisorData, CancellationToken cancellationToken = default);
        void EvaluateAndUpdateDirectoryHash(DirectoryHashData directoryHashData, string sstDirectoryHash);
    }
}