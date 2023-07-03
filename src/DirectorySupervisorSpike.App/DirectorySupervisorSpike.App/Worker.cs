using DirectorySupervisorSpike.App.configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Options;

namespace DirectorySupervisorSpike.App
{
    internal class Worker
    {
        private readonly IOptions<DirectorySupervisorOptions> directorySupervisorOptions;

        public Worker(IOptions<DirectorySupervisorOptions> directorySupervisorOptions)
        {
            this.directorySupervisorOptions = directorySupervisorOptions;
        }

        public void Execute()
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================================");
            Console.WriteLine("DirectorySupervisor configurations...");
            Console.WriteLine("==============================================");

            // var keyValuePairs = configuration.AsEnumerable().ToList();
            //foreach (var pair in keyValuePairs)
            //{
            //    Console.WriteLine($"{pair.Key} - {pair.Value}");
            //}
            //Console.WriteLine("==============================================");
            //Console.ResetColor();

            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            var patternMatchingResults = new List<string>();

            Matcher matcher = new();

            var directories = options.Directories
                .Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory.Path))
                {
                    Console.WriteLine($"Base directory path: '{directory.Path}' not found.");
                    continue;
                }

                Console.WriteLine($"Base directory path: '{directory.Path}'");
                Console.WriteLine(" include pattern:");

                /* iterate first level of directories */

                string[] sstDirectories = Directory.GetDirectories(directory.Path);

                foreach (string sstDirectory in sstDirectories)
                {
                    Console.WriteLine("");
                    Console.WriteLine($" use sst directory: '{sstDirectory}'");

                    matcher.AddIncludePatterns(options.GlobalIncludePatterns);
                    matcher.AddIncludePatterns(directory.IncludePatterns);
                    foreach (var includePattern in directory.IncludePatterns)
                    {
                        Console.WriteLine($" - {includePattern}");
                    }
                    Console.WriteLine(" exclude pattern:");
                    matcher.AddExcludePatterns(options.GlobalExcludePatterns);
                    matcher.AddExcludePatterns(directory.ExcludePatterns);
                    foreach (var excludePattern in directory.ExcludePatterns)
                    {
                        Console.WriteLine($" - {excludePattern}");
                    }

                    patternMatchingResults.AddRange(matcher.GetResultsInFullPath(sstDirectory));

                }
            }

            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine("");
            Console.WriteLine("Found Paths:");
            foreach (var patternMatchingResultFile in patternMatchingResults)
            {
                Console.WriteLine($" - {patternMatchingResultFile}");
            }

            Console.ResetColor();

        }
    }
}