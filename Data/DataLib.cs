using System.Reflection;

namespace Data
{
    public static class DataLib
    {
        public static readonly string RulesCoreDefenitions;
        public static readonly string BaisicUnitGroupsDefenitions;
        static DataLib()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();

            foreach (var resourceName in resources)
            {
                if (!resourceName.StartsWith("Data.Data.Text.", StringComparison.Ordinal))
                    continue;

                if (!resourceName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    continue;

                var fileName = resourceName.Substring(
                    "Data.Data.Text.".Length
                );

                switch (fileName)
                {
                    case "RulesCoreDefenitions.txt":
                        RulesCoreDefenitions = ReadResource(assembly, resourceName);
                        break;
                    case "BaisicUnitGroupsDefenitions.txt":
                        BaisicUnitGroupsDefenitions = ReadResource(assembly, resourceName);
                        break;
                }
            }
        }

        private static string ReadResource(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Не удалось открыть ресурс {resourceName}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
