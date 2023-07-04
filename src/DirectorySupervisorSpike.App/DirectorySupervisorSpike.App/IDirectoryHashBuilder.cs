namespace DirectorySupervisorSpike.App
{
    internal interface IDirectoryHashBuilder
    {
        string Build(string basePath, List<string> files);
    }
}