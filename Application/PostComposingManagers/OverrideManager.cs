using Application;
using Application.Settings;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObjectCacheData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class OverrideManager
{
    public static void HandleOverride()
    {
        var modType = typeof(ModConfig);
        var listProperties = modType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
                        typeof(IConfig).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]))
            .ToList();

        // Определяем максимальное количество потоков в процентах от доступных
        int maxDegree = CalculateMaxDegreeOfParallelism();

        foreach (var prop in listProperties)
        {
            var listValue = prop.GetValue(ModDataStorage.Mod) as IList;
            if (listValue == null || listValue.Count == 0) continue;

            var configs = listValue.Cast<IConfig>().ToList();

            // Собираем относительные пути модов
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

            // Удаляем найденные оверрайды
            foreach (var item in toRemove)
            {
                listValue.Remove(item);
            }
        }
    }

    private static int CalculateMaxDegreeOfParallelism()
    {
        if (ModManagerSettings.MaxPercentForParallelUsage <= 0)
            return 1;

        if (ModManagerSettings.MaxPercentForParallelUsage >= 100)
            return Environment.ProcessorCount;

        double percent = ModManagerSettings.MaxPercentForParallelUsage / 100.0;
        int threads = (int)Math.Max(1, Math.Round(Environment.ProcessorCount * percent));

        return Math.Min(threads, Environment.ProcessorCount);
    }
}