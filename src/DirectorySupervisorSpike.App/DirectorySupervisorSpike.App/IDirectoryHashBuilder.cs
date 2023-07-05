namespace DirectorySupervisorSpike.App
{
    internal interface IDirectoryHashBuilder
    {
        Task<string> BuildAsync(string basePath, List<string> files);
    }
}