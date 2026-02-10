using Application.Debugging;
using Application.Settings;
using Models.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Application.utils
{
    public static class StaticLocalisation
    {
        private static Dictionary<Language, Dictionary<string, string>> _localizations = new Dictionary<Language, Dictionary<string, string>>();
        private static bool _isLoaded = false;
        private static readonly object _lockObject = new object();

        private static Assembly FindDataAssembly()
        {
            // Попробовать найти сборку Data по имени
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var dataAssembly = assemblies.FirstOrDefault(a => 
                a.GetName().Name == "Data" && 
                a.GetManifestResourceNames().Any(n => n.Contains("Localizations")));
            
            if (dataAssembly != null)
                return dataAssembly;

            try
            {
                return Assembly.Load("Data");
            }
            catch
            {
                return null;
            }
        }

        private static void LoadLocalizations()
        {
            if (_isLoaded) return;

            lock (_lockObject)
            {
                if (_isLoaded) return;

                // Найти сборку Data
                var dataAssembly = FindDataAssembly();
                if (dataAssembly == null)
                {
                    _isLoaded = true;
                    return;
                }

                var resources = dataAssembly.GetManifestResourceNames();
                
                foreach (Language language in System.Enum.GetValues(typeof(Language)))
                {
                    var fileName = $"{language.ToString().ToLower()}.loc";
                    var resourceName = $"Data.Resources.Localizations.{fileName}";
                    
                    if (resources.Contains(resourceName))
                    {
                        try
                        {
                            using var stream = dataAssembly.GetManifestResourceStream(resourceName);
                            if (stream != null)
                            {
                                using var reader = new StreamReader(stream);
                                var json = reader.ReadToEnd();
                                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                                if (dict != null)
                                {
                                    _localizations[language] = dict;
                                }
                            }
                        }
                        catch(Exception ex) 
                        {
                            Logger.AddLog($"[MODING MANAGAER] Eternal error: {ex} trying parsing app localisation.");
                        }
                    }
                }

                _isLoaded = true;
            }
        }

        public static string GetString(string key, params object[] args)
        {
            if (!_isLoaded)
            {
                LoadLocalizations();
            }
            if (args != null && args.Length > 0)
            {
                var template = GetString(key);
                return args == null || args.Length == 0
                    ? template
                    : string.Format(template, args);
            }
           

            var language = ModManagerSettings.CurrentLanguage;
            
            if (_localizations.TryGetValue(language, out var langDict))
            {
                if (langDict.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            
            if (_localizations.TryGetValue(Language.english, out var enDict))
            {
                if (enDict.TryGetValue(key, out var enValue))
                {
                    return enValue;
                }
            }
            
            return key;
        }
    }
}

