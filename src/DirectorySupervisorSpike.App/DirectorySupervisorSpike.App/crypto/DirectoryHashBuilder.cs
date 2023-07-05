using DirectorySupervisorSpike.App.filesystem;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Security.Cryptography;
using System.Text;

namespace DirectorySupervisorSpike.App.crypto
{
    internal class DirectoryHashBuilder : IDirectoryHashBuilder
    {
        private readonly ILogger<DirectoryParser> logger;

        public DirectoryHashBuilder(ILogger<DirectoryParser> logger)
        {
            this.logger = logger;
        }

        public async Task<string> BuildDirectoryHashAsync(string directoryPath, List<string> files, CancellationToken cancellationToken = default)
        {
            var md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                var isLast = i == files.Count - 1;
                var file = files[i];

                await AppendHashFromFileAsync(directoryPath, md5, isLast, file, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (md5.Hash != null)
            {
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
            return string.Empty;
        }

        private async Task AppendHashFromFileAsync(string basePath, MD5 md5, bool isLast, string file, CancellationToken cancellationToken = default)
        {
            // hash path
            AppendHashFromRelativeFilePath(basePath, md5, file);

            // hash contents
            await AppendHashFromFileContentAsync(md5, file, isLast, cancellationToken)
                                    .ConfigureAwait(false);
        }

        private void AppendHashFromRelativeFilePath(string basePath, MD5 md5, string file)
        {
            var relativePath = file.Substring(basePath.Length + 1);
            var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);
        }

        private async Task AppendHashFromFileContentAsync(MD5 md5, string file, bool isLastFile, CancellationToken cancellationToken = default)
        {
            var contentBytes = await ReadAllBytesAsync(file, cancellationToken).ConfigureAwait(false);
            if (isLastFile)
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            else
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }

        private async Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[0];
            if (File.Exists(fileName))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        buffer = new byte[fs.Length];
                        _ = await fs.ReadAsync(buffer, 0, (int)fs.Length).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Read file error.");
                    return new byte[0];
                }

            }
            return buffer;
        }
    }
}
