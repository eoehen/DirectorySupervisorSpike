namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IHashDataManager
    {
        Task<DirectorySupervisorData> LoadJsonFileAsync(string baseDirectory);
        Task WriteJsonFileAsync(string baseDirectory, DirectorySupervisorData directorySupervisorData);
    }
}