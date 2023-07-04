using System.Security.Cryptography;
using System.Text;

namespace DirectorySupervisorSpike.App
{
    internal class DirectoryHashBuilder : IDirectoryHashBuilder
    {
        public string Build(string basePath, List<string> files)
        {
            var md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                var isLast = i == files.Count - 1;
                var file = files[i];

                AppendHashFromFile(basePath, md5, isLast, file);
            }

            if (md5.Hash != null)
            {
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
            return string.Empty;
        }

        private static void AppendHashFromFile(string basePath, MD5 md5, bool isLast, string file)
        {
            // hash path
            AppendHashFromRelativeFilePath(basePath, md5, file);

            // hash contents
            AppendHashFromFileContent(md5, file, isLast);
        }

        private static void AppendHashFromRelativeFilePath(string basePath, MD5 md5, string file)
        {
            var relativePath = file.Substring(basePath.Length + 1);
            var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);
        }

        private static void AppendHashFromFileContent(MD5 md5, string file, bool isLastFile)
        {
            var contentBytes = File.ReadAllBytes(file);
            if (isLastFile)
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            else
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }
    }
}
