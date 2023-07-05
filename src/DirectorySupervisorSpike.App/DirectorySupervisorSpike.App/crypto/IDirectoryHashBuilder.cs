namespace DirectorySupervisorSpike.App.crypto
{
    internal interface IDirectoryHashBuilder
    {
        Task<string> BuildDirectoryHashAsync(string directoryPath, List<string> files);
    }
}