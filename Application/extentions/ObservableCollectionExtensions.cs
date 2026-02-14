using Models.Configs;
using Models.EntityFiles;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Application.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static T FindById<T>(this ObservableCollection<T> collection, string targetId)
        {
            if (collection == null || string.IsNullOrEmpty(targetId))
                return default;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                return default;

            foreach (var item in collection)
            {
                var value = idProperty.GetValue(item) as string;
                if (value == targetId)
                    return item;
            }

            return default;
        }
        /// <summary>
        /// Преобразует IEnumerable<T> в ObservableCollection<T>.
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new ObservableCollection<T>();

            return new ObservableCollection<T>(source);
        }

        /// <summary>
        /// Преобразует List<T> в ObservableCollection<T>.
        /// </summary>
        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> source)
        {
            if (source == null)
                return new ObservableCollection<T>();

            return new ObservableCollection<T>(source);
        }
        public static bool RemoveById<T>(this ObservableCollection<T> collection, string targetId)
        {
            if (collection == null || string.IsNullOrEmpty(targetId))
                return false;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                return false;

            for (int i = 0; i < collection.Count; i++)
            {
                var value = idProperty.GetValue(collection[i]) as string;
                if (value == targetId)
                {
                    collection.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static bool ReplaceById<T>(this ObservableCollection<T> collection, string targetId, T newItem)
        {
            if (collection == null || string.IsNullOrEmpty(targetId) || newItem == null)
                return false;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                return false;

            for (int i = 0; i < collection.Count; i++)
            {
                var value = idProperty.GetValue(collection[i]) as string;
                if (value == targetId)
                {
                    collection[i] = newItem;
                    return true;
                }
            }

            return false;
        }

        public static void AddSafe<T>(this ObservableCollection<T> collection, T item)
        {
            if (collection == null || item == null) return;
            collection.Add(item);
        }

        public static T Random<T>(this ObservableCollection<T> collection)
        {
            if (collection == null || collection.Count == 0)
                throw new InvalidOperationException($"Коллекция пуста или null - {collection?.GetType()}.");

            Random _random = new Random();
            int index = _random.Next(collection.Count);
            return collection[index];
        }

        public static List<string> ToListString<T>(this ObservableCollection<T> collection)
        {
            if (collection == null || collection.Count == 0)
                return new List<string>();

            var result = new List<string>(collection.Count);
            foreach (var item in collection)
            {
                result.Add(item?.ToString() ?? string.Empty);
            }
            return result;
        }

        /// <summary>
        /// Перегрузка для поиска по строковому Id.
        /// </summary>
        public static T? SearchConfigInFile<T>(
            this ObservableCollection<ConfigFile<T>> configs,
            string id) where T : IConfig
        {
            return configs
                .SelectMany(cf => cf.Entities)
                .FirstOrDefault(e => e.Id.ToString() == id);
        }
        public static T? SearchConfigInFile<T>(
            this ObservableCollection<GfxFile<T>> configs,
            string id) where T : IGfx
        {
            return configs
                .SelectMany(cf => cf.Entities)
                .FirstOrDefault(e => e.Id.ToString() == id);
        }
        public static List<T> FileEntitiesToList<T>(
     this ObservableCollection<ConfigFile<T>> configs) where T : IConfig
        {
            return configs
                .SelectMany(cf => cf.Entities)
                .ToList();
        }

        public static List<T> FileEntitiesToList<T>(
    this ObservableCollection<GfxFile<T>> configs) where T : IGfx
        {
            return configs
                .SelectMany(cf => cf.Entities)
                .ToList();
        }
    }
}
