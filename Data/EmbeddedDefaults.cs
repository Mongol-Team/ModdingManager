using System.IO;
using System.Reflection;

namespace Data
{
    public static class EmbeddedDefaults
    {
        private static readonly Assembly Asm = typeof(EmbeddedDefaults).Assembly;

        public static string ReadAllText(string relativePath)
        {
            using var s = Open(relativePath);
            using var r = new StreamReader(s);
            return r.ReadToEnd();
        }

        public static Stream Open(string relativePath)
        {
            // "Configs/program.cfg" -> "Data.Configs.program.cfg"
            var ns = typeof(EmbeddedDefaults).Namespace!; // "Data"
            var resName = $"{ns}.{relativePath.Replace('\\', '/').Replace('/', '.')}";

            var stream = Asm.GetManifestResourceStream(resName);
            if (stream is null)
                throw new FileNotFoundException($"Embedded resource not found: {resName}");

            return stream;
        }

        public static bool Exists(string relativePath)
        {
            try
            {
                var ns = typeof(EmbeddedDefaults).Namespace!;
                var resName = $"{ns}.{relativePath.Replace('\\', '/').Replace('/', '.')}";
                var stream = Asm.GetManifestResourceStream(resName);
                return stream != null;
            }
            catch
            {
                return false;
            }
        }
    }
}

