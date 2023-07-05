namespace DirectorySupervisorSpike.App
{
    internal interface IDirectoryHashBuilder
    {
        Task<string> BuildDirectoryHashAsync(string directoryPath, List<string> files);
    }
}