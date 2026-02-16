using Application;
using Application.Settings;
using Application.utils.Math;
using Application.utils.Pathes;
using Models.Configs;
using Models.EntityFiles;
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
        int maxDegree = ParallelTaskCounter.CalculateMaxDegreeOfParallelism();
        var modType = typeof(ModConfig);

        var listProperties = modType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        typeof(IEnumerable).IsAssignableFrom(p.PropertyType) &&
                        typeof(IFile).IsAssignableFrom(p.PropertyType.GetGenericArguments()[0]))
            .ToList();

        foreach (var prop in listProperties)
        {
            var value = prop.GetValue(ModDataStorage.Mod);

            var files = (value as IEnumerable)?.Cast<IFile>().ToList();
            if (files == null || files.Count == 0) continue;

            var modRelatives = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                if (string.IsNullOrEmpty(file.FileFullPath)) continue;

                if (file.FileFullPath.StartsWith(ModPathes.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    string rel = file.FileFullPath
                        .Substring(ModPathes.RootPath.Length)
                        .Replace('\\', '/')
                        .TrimStart('/');
                    modRelatives.Add(rel);
                }
            }

            var toRemove = new List<IFile>();

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = maxDegree }, file =>
            {
                if (string.IsNullOrEmpty(file.FileFullPath)) return;

                if (file.FileFullPath.StartsWith(GamePathes.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    string rel = file.FileFullPath
                        .Substring(GamePathes.RootPath.Length)
                        .Replace('\\', '/')
                        .TrimStart('/');

                    if (modRelatives.Contains(rel))
                    {
                        lock (toRemove)
                        {
                            toRemove.Add(file);
                        }
                    }
                }
            });

            if (value is IList list)
            {
                foreach (var item in toRemove)
                {
                    if (list.Contains(item)) // проверка на наличие
                    {
                        list.Remove(item);
                    }
                }
            }
        }
    }
}
