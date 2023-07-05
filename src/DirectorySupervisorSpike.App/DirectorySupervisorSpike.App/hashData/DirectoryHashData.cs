namespace DirectorySupervisorSpike.App.hashData
{
    public class DirectoryHashData
    {
        public string DirectoryPath { get; }

        public DirectoryHashData(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        public string? LastDirectoryHash { get; set; }
        public string? CurrentDirectoryHash { get; set; }
        public DateTime LastDirectoryHashDifferentSince { get; set; }
        public DateTime LastHashCheck { get; set; }
        public bool ImportPending { get; set; } = false;
    }
}
