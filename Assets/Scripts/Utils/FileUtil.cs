using System.IO;
using ByteSizeLib;

namespace Utils
{
    public static class FileUtil
    {
        public static void CopyFile(string sourcePath, string destPath, bool overwrite = false)
        {
            var destDirName = Path.GetDirectoryName(destPath);
            Directory.CreateDirectory(destDirName);
            File.Copy(sourcePath, destPath, overwrite);
        }

        public static float GetFileSizeInMb(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var bytes = fileInfo.Length;
            var byteSize = ByteSize.FromBytes(bytes);
            return (float)byteSize.MegaBytes;
        }
    }
}