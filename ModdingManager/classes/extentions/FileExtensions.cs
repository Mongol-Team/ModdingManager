using System.IO;

namespace ModdingManager.classes.extentions
{
    public static class FileExtensions
    {
        public static string CopyFileToTemp(this FileInfo file, string tempDir)
        {
            if (!file.Exists)
                throw new FileNotFoundException("Файл не найден", file.FullName);

            Directory.CreateDirectory(tempDir);

            string destPath = Path.Combine(tempDir, file.Name);
            file.CopyTo(destPath, overwrite: true);

            return destPath;
        }
    }
}
