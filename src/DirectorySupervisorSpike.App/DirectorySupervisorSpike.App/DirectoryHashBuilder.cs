using System.Security.Cryptography;
using System.Text;

namespace DirectorySupervisorSpike.App
{
    internal class DirectoryHashBuilder : IDirectoryHashBuilder
    {
        public async Task<string> BuildAsync(string basePath, List<string> files)
        {
            var md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                var isLast = i == files.Count - 1;
                var file = files[i];

                await AppendHashFromFileAsync(basePath, md5, isLast, file)
                    .ConfigureAwait(false);
            }

            if (md5.Hash != null)
            {
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
            return string.Empty;
        }

        private static async Task AppendHashFromFileAsync(string basePath, MD5 md5, bool isLast, string file)
        {
            // hash path
            AppendHashFromRelativeFilePath(basePath, md5, file);

            // hash contents
            await AppendHashFromFileContentAsync(md5, file, isLast)
                                    .ConfigureAwait(false);
        }

        private static void AppendHashFromRelativeFilePath(string basePath, MD5 md5, string file)
        {
            var relativePath = file.Substring(basePath.Length + 1);
            var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);
        }

        private static async Task AppendHashFromFileContentAsync(MD5 md5, string file, bool isLastFile)
        {
            var contentBytes = await ReadAllBytesAsync(file).ConfigureAwait(false);
            if (isLastFile)
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            else
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }

        private static async Task<byte[]> ReadAllBytesAsync(string fileName)
        {
            byte[]? buffer = null;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                _ = await fs.ReadAsync(buffer, 0, (int)fs.Length).ConfigureAwait(false);
            }
            return buffer;
        }
    }
}
