namespace DirectorySupervisorSpike.App.configuration
{
    internal class DirectorySupervisorOptions
    {
        public List<string> GlobalIncludePatterns { get; } = new List<string>();

        public List<string> GlobalExcludePatterns { get; } = new List<string>();

        public List<DirectorySupervisorDirOptions> Directories { get; } = new List<DirectorySupervisorDirOptions>();
    }

    internal class DirectorySupervisorDirOptions
    {
        public string? Path { get; set; }

        public List<string> IncludePatterns { get; } = new List<string>();

        public List<string> ExcludePatterns { get; } = new List<string>();
    }

}
