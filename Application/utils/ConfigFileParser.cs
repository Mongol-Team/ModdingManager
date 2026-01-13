using Application.Settings;
using System.Text;

namespace Application.Utils
{
    public static class ConfigFileParser
    {
        public static Dictionary<string, string> ParseConfigFile(string FileFullPath)
        {
            var config = new Dictionary<string, string>();
            if (!File.Exists(FileFullPath)) return config;

            foreach (var line in File.ReadAllLines(FileFullPath, Encoding.UTF8))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed[0] == '#' || trimmed[0] == ';') continue;

                var idx = trimmed.IndexOf('=');
                if (idx <= 0) continue;

                var key = trimmed.Substring(0, idx).Trim();
                var value = trimmed.Substring(idx + 1).Trim();
                if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                    value = value.Substring(1, value.Length - 2);

                config[key] = value;
            }

            return config;
        }

        public static void WriteConfigFile(string FileFullPath, Dictionary<string, string> config)
        {
            var dir = Path.GetDirectoryName(FileFullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var lines = new List<string>
            {
                "# Configuration file",
                $"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                ""
            };

            foreach (var kvp in config.OrderBy(x => x.Key))
            {
                var value = kvp.Value;
                if (value.Any(c => c == ' ' || c == '=' || c == '#' || c == ';'))
                    value = $"\"{value}\"";
                lines.Add($"{kvp.Key}={value}");
            }

            File.WriteAllLines(FileFullPath, lines, Encoding.UTF8);
        }

        public static List<string> ParseList(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<string>();
            if (value.StartsWith("[") && value.EndsWith("]"))
                value = value.Substring(1, value.Length - 2);

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().Trim('"'))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        public static string SerializeList(List<string> list) =>
            list == null || list.Count == 0 ? "[]" : "[" + string.Join(",", list.Select(s => $"\"{s}\"")) + "]";

        public static List<RecentProject> ParseRecentProjects(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new List<RecentProject>();
            if (value.StartsWith("[") && value.EndsWith("]"))
                value = value.Substring(1, value.Length - 2);

            if (string.IsNullOrWhiteSpace(value)) return new List<RecentProject>();

            if (value.Contains("\"Path\""))
            {
                return value.Split(new[] { "},{" }, StringSplitOptions.None)
                    .Select(item =>
                    {
                        var clean = item.Trim().TrimStart('{').TrimEnd('}');
                        var parts = clean.Split(',');
                        string path = string.Empty, name = string.Empty;

                        foreach (var part in parts)
                        {
                            var kvp = part.Split(':', 2);
                            if (kvp.Length != 2) continue;

                            var key = kvp[0].Trim().Trim('"');
                            var val = kvp[1].Trim().Trim('"');
                            if (key.Equals("Path", StringComparison.OrdinalIgnoreCase)) path = val;
                            else if (key.Equals("Name", StringComparison.OrdinalIgnoreCase)) name = val;
                        }

                        return string.IsNullOrEmpty(path) ? null : new RecentProject(path, name);
                    })
                    .Where(p => p != null)
                    .ToList()!;
            }

            return ParseList("[" + value + "]")
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => new RecentProject(p, Path.GetFileName(p) ?? p))
                .ToList();
        }

        public static string SerializeRecentProjects(List<RecentProject> projects) =>
            projects == null || projects.Count == 0
                ? "[]"
                : "[" + string.Join(",", projects.Select(p => $"{{\"Path\":\"{p.Path}\",\"Name\":\"{p.Name}\"}}")) + "]";
    }
}
