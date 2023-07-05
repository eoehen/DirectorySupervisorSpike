namespace DirectorySupervisorSpike.App.hashData
{
    public class DirectoryHashData
    {
        public string DirectoryName { get; }

        public DirectoryHashData(string directoryName)
        {
            DirectoryName = directoryName;
        }

        public string? LastDirectoryHash { get; set; }
        public string? CurrentDirectoryHash { get; set; }
        public DateTime LastDirectoryHashDifferentSince { get; set; }
        public DateTime LastHashCheck { get; set; }
        public bool ImportPending { get; set; } = false;
    }
}
