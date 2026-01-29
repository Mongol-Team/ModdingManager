using System;
using System.ComponentModel;
using System.Reflection;

namespace Data
{
    public static class DataLib
    {
        public static readonly string RulesCoreDefenitions;
        public static readonly string BaisicUnitGroupsDefenitions;
        public static readonly string ErrorTypesDefenitions;
        static DataLib()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();

            foreach (var resourceName in resources)
            {
                if (resourceName.StartsWith("Data.Embedded.Text.", StringComparison.Ordinal) &&
                    resourceName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = resourceName.Substring("Data.Embedded.Text.".Length);

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

                if (resourceName.StartsWith("Data.Embedded.JSON.", StringComparison.Ordinal) &&
                    resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = resourceName.Substring("Data.Embedded.JSON.".Length);

                    switch (fileName)
                    {
                        case "ErrorTypesDefenitions.json":
                            ErrorTypesDefenitions = ReadResource(assembly, resourceName);
                            break;
                        
                    }
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
