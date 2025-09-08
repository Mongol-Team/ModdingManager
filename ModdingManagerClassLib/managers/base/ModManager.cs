using ModdingManager.classes.utils;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels;
using SixLabors.ImageSharp;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace ModdingManager.managers.@base
{
    public class ModManager
    {
        public static string ModDirectory;
        public static bool IsDebugRuning;
        public static string GameDirectory;
        public static string CurrentLanguage = "russian";
        public static ModConfig CurrentConfig = new ();
        public ModManager()
        {
            OnLoaded();
        }
        private void OnLoaded()
        {
            string relativePath = System.IO.Path.Combine("..", "..", "..", "data", "dir.json");
            string fullPath = System.IO.Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);
            Logger.LoggingLevel = 3;
            try
            {
                string json = File.ReadAllText(fullPath);
                var path = JsonSerializer.Deserialize<PathConfig>(json);
                ModDirectory = path.ModPath;
                GameDirectory = path.GamePath;
                //ModConfig.LoadInstance();

                Logger.AddLog(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")));
                Logger.AddLog(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage)
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage)));
                Logger.AddLog(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage)
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage)));
                Logger.AddLog(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace")
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace")));

            }
            catch (Exception ex)
            {
                Logger.AddLog($"[MAIN WPF] On load exception: {ex.Message}{ex.StackTrace}");
            }
        }
        public static List<string> LoadCountryFileNames()
        {
            string countriesDir = Path.Combine(ModManager.ModDirectory, "common", "country_tags");

            if (!System.IO.Directory.Exists(countriesDir))
            {
                System.Windows.MessageBox.Show("Папка 'countries' не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var filePaths = System.IO.Directory.GetFiles(countriesDir);
            var fileNamesLines = filePaths.Select(path => Path.GetFileName(path)).ToList();
            return fileNamesLines;
        }
        public static System.Windows.Media.Color GenerateWpfColorFromId(int id)
        {
            byte r = (byte)((id * 53) % 255);
            byte g = (byte)((id * 97) % 255);
            byte b = (byte)((id * 151) % 255);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }
        public static System.Drawing.Color GenerateColorFromId(int id)
        {
            byte r = (byte)((id * 53) % 255);
            byte g = (byte)((id * 97) % 255);
            byte b = (byte)((id * 151) % 255);
            return System.Drawing.Color.FromArgb(r, g, b);
        }
        public static System.Windows.Media.Color ParseColor(string content)
        {
            string[] parts = content.Trim('{', '}').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return Colors.Black;

            return System.Windows.Media.Color.FromRgb(
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2])
            );
        }
    }
}