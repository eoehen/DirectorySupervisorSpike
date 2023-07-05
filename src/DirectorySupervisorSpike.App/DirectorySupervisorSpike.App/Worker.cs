using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.crypto;
using DirectorySupervisorSpike.App.filesystem;
using DirectorySupervisorSpike.App.hashData;
using DirectorySupervisorSpike.App.performance;
using Figgle;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectorySupervisorSpike.App
{
    internal class Worker
    {
        private readonly ILogger<Worker> logger;
        private readonly IOptions<DirectorySupervisorOptions> directorySupervisorOptions;
        private readonly ISstDirectoryHashCalculator sstDirectoryHashCalculator;
        private readonly IHashDataManager hashDataManager;

        public Worker(
            ILogger<Worker> logger,
            IOptions<DirectorySupervisorOptions> directorySupervisorOptions,
            ISstDirectoryHashCalculator sstDirectoryHashCalculator,
            IDirectoryParser directoryParser,
            IHashDataManager hashDataManager,
            IDirectoryHashBuilder hashBuilder)
        {
            this.logger = logger;
            this.directorySupervisorOptions = directorySupervisorOptions;
            this.sstDirectoryHashCalculator = sstDirectoryHashCalculator;
            this.hashDataManager = hashDataManager;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine(FiggleFonts.Standard.Render("exanic"));
            Console.WriteLine(FiggleFonts.Standard.Render("DirectorySupervisor"));

            await SuperviseDirectoryAsync();
            Console.WriteLine("... next check in 10 seconds.");

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync())
            {
                await SuperviseDirectoryAsync();
                Console.WriteLine("... next check in 10 seconds.");
            }
        }

        private async Task SuperviseDirectoryAsync()
        {
            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            var directories = options.Directories
                .Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();

            foreach (var directoryOptions in directories)
            {
                var directoryTimepiece = Timepiece.StartNew();

                var directorySupervisorData = await hashDataManager
                    .LoadJsonFileAsync(directoryOptions.Path)
                    .ConfigureAwait(false);

                foreach (var directoryHashData in directorySupervisorData.DirectoryHashDatas)
                {
                    var sstDirTimepiece = Timepiece.StartNew();

                    // Calc directory hash
                    var sstDirectoryHash = 
                        await sstDirectoryHashCalculator.CalcDirectoryHashAsync(directoryOptions, directoryHashData)
                        .ConfigureAwait(false);

                    // Evaluate and update directory hash data
                    hashDataManager.EvaluateAndUpdateDirectoryHash(directoryHashData, sstDirectoryHash);

                    await hashDataManager.WriteJsonFileAsync(directoryOptions.Path, directorySupervisorData).ConfigureAwait(false);

                    logger.LogDebug($"SST directory path: '{directoryHashData.DirectoryPath}' elapsed {sstDirTimepiece.GetElapsedTime()}");
                }

                logger.LogInformation($"Directory hash checked '{directoryOptions.Path}' elapsed {directoryTimepiece.GetElapsedTime()}");
            }
        }
    }
}