using DirectorySupervisorSpike.App.configuration;
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
            IHashDataManager hashDataManager)
        {
            this.logger = logger;
            this.directorySupervisorOptions = directorySupervisorOptions;
            this.sstDirectoryHashCalculator = sstDirectoryHashCalculator;
            this.hashDataManager = hashDataManager;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine(FiggleFonts.Standard.Render("exanic"));
            Console.WriteLine(FiggleFonts.Standard.Render("DirectorySupervisor"));

            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            await SuperviseDirectoryAsync(cancellationToken);
            Console.WriteLine($"... next check in {options.CheckIntervalSeconds} seconds.");

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.CheckIntervalSeconds));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await SuperviseDirectoryAsync();
                Console.WriteLine($"... next check in {options.CheckIntervalSeconds} seconds.");
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private async Task SuperviseDirectoryAsync(CancellationToken cancellationToken = default)
        {
            var options = directorySupervisorOptions?.Value;
            if (options == null) { return; }

            var directories = options.Directories
                .Where(x => !string.IsNullOrWhiteSpace(x.Path)).ToList();

            long memory = GC.GetTotalMemory(true);
            var kbMemory = memory > 0 ? memory / 1024 : 0;
            logger.LogInformation($"current memory usage {kbMemory} kb.");

            foreach (var directoryOptions in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var directoryTimepiece = Timepiece.StartNew();

                var directorySupervisorData = await hashDataManager
                    .LoadJsonFileAsync(directoryOptions)
                    .ConfigureAwait(false);

                foreach (var directoryHashData in directorySupervisorData.DirectoryHashDatas)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var sstDirTimepiece = Timepiece.StartNew();

                    // Calc directory hash
                    var sstDirectoryHash = 
                        await sstDirectoryHashCalculator.CalcDirectoryHashAsync(directoryOptions, directoryHashData, cancellationToken)
                        .ConfigureAwait(false);

                    // Evaluate and update directory hash data
                    hashDataManager.EvaluateAndUpdateDirectoryHash(directoryHashData, sstDirectoryHash);

                    await hashDataManager.WriteJsonFileAsync(directoryOptions, directorySupervisorData, cancellationToken)
                        .ConfigureAwait(false);

                    logger.LogDebug($"SST directory path: '{directoryHashData.DirectoryPath}' elapsed {sstDirTimepiece.GetElapsedTime()}");
                }

                logger.LogInformation($"Directory hash checked '{directoryOptions.Path}' elapsed {directoryTimepiece.GetElapsedTime()}");
            }
        }
    }
}