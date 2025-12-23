using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Application.Utils
{
    public static class ConfigFileParser
    {
        public static Dictionary<string, string> ParseConfigFile(string filePath)
        {
            var config = new Dictionary<string, string>();
            
            if (!File.Exists(filePath))
                return config;
            
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";"))
                    continue;
                
                var equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex <= 0)
                    continue;
                
                var key = trimmedLine.Substring(0, equalIndex).Trim();
                var value = trimmedLine.Substring(equalIndex + 1).Trim();
                
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                config[key] = value;
            }
            
            return config;
        }
        
        public static void WriteConfigFile(string filePath, Dictionary<string, string> config)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var lines = new List<string>();
            lines.Add("# Configuration file");
            lines.Add($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            lines.Add("");
            
            foreach (var kvp in config.OrderBy(x => x.Key))
            {
                var value = kvp.Value;
                if (value.Contains(" ") || value.Contains("=") || value.Contains("#") || value.Contains(";"))
                {
                    value = $"\"{value}\"";
                }
                lines.Add($"{kvp.Key}={value}");
            }
            
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
        
        public static List<string> ParseList(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<string>();
            
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(1, value.Length - 2);
            }
            
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().Trim('"'))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        
        public static string SerializeList(List<string> list)
        {
            if (list == null || list.Count == 0)
                return "[]";
            
            return "[" + string.Join(",", list.Select(s => $"\"{s}\"")) + "]";
        }
    }
}

