namespace DirectorySupervisorSpike.App.configuration
{
    internal class DirectorySupervisorOptions
    {
        public int CheckIntervalSeconds { get; set; } = 10;

        public int ChangeDetectionDelayMinutes { get; set; } = 1;

        public List<string> GlobalIncludePatterns { get; } = new List<string>();

        public List<string> GlobalExcludePatterns { get; } = new List<string>();

        public List<DirectorySupervisorDirOptions> Directories { get; } = new List<DirectorySupervisorDirOptions>();
    }

    internal class DirectorySupervisorDirOptions
    {
        public string Path { get; }
        public bool UseSubDirectories { get; set; } = true;

        public DirectorySupervisorDirOptions(string path)
        {
            Path = path;
        }

        public List<string> IncludePatterns { get; } = new List<string>();

        public List<string> ExcludePatterns { get; } = new List<string>();
    }

}
