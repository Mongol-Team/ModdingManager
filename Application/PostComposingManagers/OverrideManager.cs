using Application;
using Application.Debugging;
using Application.utils;
using Application.utils.Math;
using Application.utils.Pathes;
using Models.Configs;
using Models.EntityFiles;
using Models.Interfaces;
using System.Collections;

public static class OverrideManager
{
    public static void HandleOverride(ModConfig modConfig)
    {
        Logger.AddLog(StaticLocalisation.GetString("Log.OverrideManager.Started"));

        int maxDegree = ParallelTaskCounter.CalculateMaxDegreeOfParallelism();
        var modType = typeof(ModConfig);

        // Обработка обычных списков IFile
        ProcessFileCollections(modType, maxDegree);

        // Обработка политических карт
        ProcessPoliticalMaps(modType, maxDegree);

        Logger.AddLog(StaticLocalisation.GetString("Log.OverrideManager.Completed"));
    }

    private static void ProcessFileCollections(Type modType, int maxDegree)
    {
        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingFileCollections"));

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

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingProperty", prop.Name, files.Count));

            // Собираем относительные пути из мода
            var modRelatives = CollectModRelativePaths(files);

            // Находим файлы для удаления (оверрайд по пути)
            var toRemove = FindFilesToRemove(files, modRelatives, maxDegree);

            // Удаляем найденные файлы
            RemoveFilesFromCollection(value, toRemove);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.RemovedFiles", toRemove.Count, prop.Name));
        }
    }

    private static void ProcessPoliticalMaps(Type modType, int maxDegree)
    {
        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingPoliticalMaps"));

        var mapProperties = modType.GetProperties()
            .Where(p => typeof(IPoliticalMap).IsAssignableFrom(p.PropertyType))
            .ToList();

        foreach (var prop in mapProperties)
        {
            var politicalMap = prop.GetValue(ModDataStorage.Mod) as IPoliticalMap;
            if (politicalMap == null) continue;

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingMap", prop.Name));

            // Обработка базовых сущностей
            if (politicalMap.Basic != null)
            {
                ProcessBasicMapEntities(politicalMap.Basic, "Basic");
            }

            switch(politicalMap)
            {
                case MapConfig mapConfig:
                    var sratRegsDubles = FindConfigDuplicatesById(
                        mapConfig.StrategicRegions
                                 .SelectMany(f => f.Entities)
                                 .Cast<IConfig>()
                                 .ToList());

                    mapConfig.StrategicRegions.RemoveAll(
                        file => file.Entities.Any(entity => sratRegsDubles.Contains(entity))
                    );

                    break;


            }
            var layers = 1;
        }
    }

    private static void ProcessBasicMapEntities(IEnumerable<IBasicMapEntity> entities, string layerName)
    {
        if (entities == null) return;

        var entitiesList = entities.ToList();
        if (entitiesList.Count == 0) return;

        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingBasicEntities", layerName, entitiesList.Count));

        // Проверяем, реализуют ли сущности IConfig
        var configs = entitiesList.OfType<IConfig>().ToList();
        if (configs.Count == 0)
        {
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.NoConfigsFound", layerName));
            return;
        }

        // Удаляем дубликаты по Id
        var toRemove = FindConfigDuplicatesById(configs);

        // Удаляем из исходной коллекции
        if (entities is IList list)
        {
            foreach (var item in toRemove)
            {
                if (list.Contains(item))
                {
                    list.Remove(item);
                }
            }
        }

        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.RemovedBasicEntities", toRemove.Count, layerName));
    }

    private static void ProcessLayerEntities(IEnumerable<IMapEntity> entities, string layerName, int maxDegree)
    {
        if (entities == null) return;

        var entitiesList = entities.ToList();
        if (entitiesList.Count == 0) return;

        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.ProcessingLayerEntities", layerName, entitiesList.Count));

        // IMapEntity теперь наследуется от IConfig, поэтому приводим напрямую
        var configs = entitiesList.Cast<IConfig>().ToList();

        // Удаляем дубликаты по Id
        var toRemove = FindConfigDuplicatesById(configs);

        // Удаляем из исходной коллекции, если она изменяемая
        if (entities is IList<IMapEntity> genericList)
        {
            foreach (var item in toRemove.OfType<IMapEntity>())
            {
                if (genericList.Contains(item))
                {
                    genericList.Remove(item);
                }
            }
        }
        else if (entities is IList list)
        {
            foreach (var item in toRemove)
            {
                if (list.Contains(item))
                {
                    list.Remove(item);
                }
            }
        }

        Logger.AddDbgLog(StaticLocalisation.GetString("Log.OverrideManager.RemovedLayerEntities", toRemove.Count, layerName));
    }

    private static List<IConfig> FindConfigDuplicatesById(List<IConfig> configs)
    {
        var toRemove = new List<IConfig>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Сначала собираем все Id из мода
        var modIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var config in configs)
        {
            if (config.Id == null || string.IsNullOrEmpty(config.Id.ToString())) continue;

            if (string.IsNullOrEmpty(config.FileFullPath)) continue;

            if (config.FileFullPath.StartsWith(ModPathes.RootPath, StringComparison.OrdinalIgnoreCase))
            {
                modIds.Add(config.Id.ToString());
            }
        }

        // Затем удаляем дубликаты
        foreach (var config in configs)
        {
            if (config.Id == null || string.IsNullOrEmpty(config.Id.ToString())) continue;

            if (string.IsNullOrEmpty(config.FileFullPath)) continue;

            var idString = config.Id.ToString();

            // Если это файл из игры и его Id есть в моде - удаляем
            if (config.FileFullPath.StartsWith(GamePathes.RootPath, StringComparison.OrdinalIgnoreCase)
                && modIds.Contains(idString))
            {
                toRemove.Add(config);
                continue;
            }

            // Удаляем полные дубликаты по Id (если встречается второй раз)
            if (seenIds.Contains(idString))
            {
                toRemove.Add(config);
            }
            else
            {
                seenIds.Add(idString);
            }
        }

        return toRemove;
    }

    private static HashSet<string> CollectModRelativePaths(IEnumerable<IFile> files)
    {
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

        return modRelatives;
    }

    private static List<IFile> FindFilesToRemove(IEnumerable<IFile> files, HashSet<string> modRelatives, int maxDegree)
    {
        var toRemove = new List<IFile>();
        var filesList = files.ToList();

        Parallel.ForEach(filesList, new ParallelOptions { MaxDegreeOfParallelism = maxDegree }, file =>
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

        return toRemove;
    }

    private static void RemoveFilesFromCollection(object collection, List<IFile> toRemove)
    {
        if (collection is IList list)
        {
            foreach (var item in toRemove)
            {
                if (list.Contains(item))
                {
                    list.Remove(item);
                }
            }
        }
    }
}