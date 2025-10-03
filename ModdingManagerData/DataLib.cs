using System.Reflection;

namespace ModdingManagerData
{
    public static class DataLib
    {
        public static readonly string RulesCoreDefenitions = "Null";

        static DataLib()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using Stream? stream = assembly.GetManifestResourceStream("ModdingManagerData.Data.Rules.RulesCoreDefenitions.txt");
            if (stream == null)
                throw new InvalidOperationException("Не найден embedded ресурс RulesCoreDefenitions.txt");

            using var reader = new StreamReader(stream);
            RulesCoreDefenitions = reader.ReadToEnd();
        }
    }
}
