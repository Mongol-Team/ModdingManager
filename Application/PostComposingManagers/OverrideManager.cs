using Application;
using Application.Settings;
using Application.utils.Math;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObjectCacheData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

public static class OverrideManager
{
    public static void HandleOverride()
    {
        int maxDegree = ParallelTheadCounter.CalculateMaxDegreeOfParallelism();
        var modType = typeof(ModConfig);
        var listProperties = modType.GetProperties()
      .Where(p => p.PropertyType.IsGenericType &&
                  typeof(IEnumerable).IsAssignableFrom(p.PropertyType) &&
                  typeof(IConfig).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]))
      .ToList();

        foreach (var prop in listProperties)
        {
            var value = prop.GetValue(ModDataStorage.Mod);

            // Приводим к IEnumerable<IConfig>
            var configs = (value as IEnumerable)?.Cast<IConfig>().ToList();
            if (configs == null || configs.Count == 0) continue;



            var modRelatives = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var config in configs)
            {
                if (string.IsNullOrEmpty(config.FileFullPath)) continue;

                if (config.FileFullPath.StartsWith(ModPathes.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    string rel = config.FileFullPath
                        .Substring(ModPathes.RootPath.Length)
                        .Replace('\\', '/')
                        .TrimStart('/');
                    modRelatives.Add(rel);
                }
            }

            // Ищем конфиги из игры, которые нужно удалить
            var toRemove = new List<IConfig>();

            Parallel.ForEach(configs, new ParallelOptions { MaxDegreeOfParallelism = maxDegree }, config =>
            {
                if (string.IsNullOrEmpty(config.FileFullPath)) return;

                if (config.FileFullPath.StartsWith(GamePathes.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    string rel = config.FileFullPath
                        .Substring(GamePathes.RootPath.Length)
                        .Replace('\\', '/')
                        .TrimStart('/');

                    if (modRelatives.Contains(rel))
                    {
                        lock (toRemove)
                        {
                            toRemove.Add(config);
                        }
                    }
                }
            });

            foreach (var item in toRemove)
            {
                if (value is IList list)
                {
                    list.Remove(item);
                }
                else if (value is ICollection<IConfig> coll)
                {
                    coll.Remove(item);
                }
            }

        }
    }

    
}