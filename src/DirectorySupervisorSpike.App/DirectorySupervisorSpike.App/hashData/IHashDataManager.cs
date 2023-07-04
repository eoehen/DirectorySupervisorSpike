namespace DirectorySupervisorSpike.App.hashData
{
    internal interface IHashDataManager
    {
        DirectorySupervisorData LoadJsonFile(string baseDirectory);
        void WriteJsonFile(string baseDirectory, DirectorySupervisorData directorySupervisorData);
    }
}