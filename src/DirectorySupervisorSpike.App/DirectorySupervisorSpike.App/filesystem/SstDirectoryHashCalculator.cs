using DirectorySupervisorSpike.App.configuration;
using DirectorySupervisorSpike.App.crypto;
using DirectorySupervisorSpike.App.hashData;

namespace DirectorySupervisorSpike.App.filesystem
{
    internal class SstDirectoryHashCalculator : ISstDirectoryHashCalculator
    {
        private readonly IDirectoryParser directoryParser;
        private readonly IDirectoryHashBuilder hashBuilder;

        public SstDirectoryHashCalculator(
            IDirectoryParser directoryParser,
            IDirectoryHashBuilder hashBuilder)
        {
            this.directoryParser = directoryParser;
            this.hashBuilder = hashBuilder;
        }

        public async Task<string> CalcDirectoryHashAsync(DirectorySupervisorDirOptions directoryOptions, DirectoryHashData directoryHashData)
        {
            // Read all Files in directory
            List<string> directoryFiles = directoryParser
                .ParseDirectoryFiles(directoryHashData.DirectoryPath, directoryOptions);

            // Create hash for all files in directory
            var sstDirectoryHash = await hashBuilder
                .BuildDirectoryHashAsync(directoryHashData.DirectoryPath, directoryFiles)
                .ConfigureAwait(false);

            return sstDirectoryHash;
        }
    }
}
