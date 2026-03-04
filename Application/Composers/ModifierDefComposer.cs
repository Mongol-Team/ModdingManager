using Application.Debugging;
using Application.Extentions;
using Application.utils.Pathes;
using Models.Configs;
using Models.EntityFiles;
using Models.Enums;
using Models.Types.HtmlFilesData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Diagnostics;

namespace Application.Composers
{
    public class ModifierDefComposer
    {
        /// <summary>
        /// Парсит все файлы определений модификаторов и возвращает список файлов (ConfigFile<ModifierDefinitionConfig>)
        /// </summary>
        public static List<ConfigFile<ModifierDefinitionConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var modifierFiles = new List<ConfigFile<ModifierDefinitionConfig>>();

            // 1. Обычные txt-файлы с определениями (01_modifier_definitions и т.п.)
            string[] possibleDefPaths =
            {
                ModPathes.ModifierDefFirstPath,
                GamePathes.ModifierHtmlPath   // ← в оригинале был GamePathes.ModifierHtmlPath, но это может быть опечатка — оставил как есть
            };

            foreach (string path in possibleDefPaths)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.ModifierDefFirstPath);

                        var configFile = ParseTxtFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            modifierFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл определений модификаторов: {configFile.FileName} → {configFile.Entities.Count} определений");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"Ошибка парсинга файла определений {file}: {ex.Message}");
                    }
                }

                if (modifierFiles.Any()) break; // если нашли в моде — не ищем в игре
            }

            // 2. HTML / MD файлы — обрабатываем как core-определения
            ProcessDocumentationFiles(modifierFiles);

            stopwatch.Stop();
            Logger.AddLog($"Парсинг определений модификаторов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {modifierFiles.Count}, определений всего: {modifierFiles.Sum(f => f.Entities.Count)}");

            return modifierFiles;
        }

        private static ConfigFile<ModifierDefinitionConfig> ParseTxtFile(HoiFuncFile file, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<ModifierDefinitionConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket bracket in file.Brackets)
            {
                var cfg = ParseModifierDefConfig(bracket, fileFullPath, isOverride);
                if (cfg != null)
                {
                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлено определение модификатора: {cfg.Id}");
                }
            }

            return configFile;
        }

        private static ModifierDefinitionConfig ParseModifierDefConfig(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var config = new ModifierDefinitionConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Var v in bracket.SubVars)
            {
                switch (v.Name)
                {
                    case "value_type":
                        config.ValueType = v.Value.ToString().SnakeToPascal().ToEnum<ModifierDefenitionValueType>(default);
                        break;

                    case "precision":
                        config.Precision = v.Value.ToInt();
                        break;

                    case "cathegory":
                        config.Cathegory = v.Value.ToString().SnakeToPascal().ToEnum<ModifierDefinitionCathegoryType>(default);
                        break;

                    case "color_type":
                        config.ColorType = v.Value.ToString().SnakeToPascal().ToEnum<ModifierDefenitionColorType>(default);
                        break;
                }
            }

            return config;
        }

        /// <summary>
        /// Обрабатывает HTML и MD файлы документации как core-определения
        /// </summary>
        private static void ProcessDocumentationFiles(List<ConfigFile<ModifierDefinitionConfig>> modifierFiles)
        {
            string[] possibleHtmlPaths =
            {
                ModPathes.ModifierHtmlPath,
                GamePathes.ModifierHtmlPath
            };

            string[] possibleMdPaths =
            {
                ModPathes.ModifiersMdPath,
                GamePathes.ModifiersMdPath
            };

            List<ModifierDefinitionConfig> docModifiers = new();

            // Сначала пробуем HTML
            foreach (string path in possibleHtmlPaths)
            {
                if (!File.Exists(path)) continue;

                try
                {
                    string content = File.ReadAllText(path);
                    var parser = new HtmlModifierParser();
                    var parsedFile = parser.Parse(content) as ModifierDefinitionFile;

                    if (parsedFile?.ModifierDefinitions?.Any() == true)
                    {
                        docModifiers.AddRange(parsedFile.ModifierDefinitions);
                        Logger.AddDbgLog($"Загружены core-определения из HTML: {path} → {docModifiers.Count} модификаторов");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLog($"Ошибка парсинга HTML-документации {path}: {ex.Message}");
                }
            }

            // Если HTML не дал результата — пробуем MD
            if (!docModifiers.Any())
            {
                foreach (string path in possibleMdPaths)
                {
                    if (!File.Exists(path)) continue;

                    try
                    {
                        string content = File.ReadAllText(path);
                        var parser = new MdModifierParser();
                        var parsedFile = parser.Parse(content) as ModifierDefinitionFile;

                        if (parsedFile?.ModifierDefinitions?.Any() == true)
                        {
                            docModifiers.AddRange(parsedFile.ModifierDefinitions);
                            Logger.AddDbgLog($"Загружены core-определения из MD: {path} → {docModifiers.Count} модификаторов");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"Ошибка парсинга MD-документации {path}: {ex.Message}");
                    }
                }
            }

            if (!docModifiers.Any())
            {
                Logger.AddLog("Не найдены валидные core-определения модификаторов в HTML/MD файлах");
                return;
            }

            var coreFile = ModDataStorage.Mod.ModifierDefinitions?.FirstOrDefault(f => f.FileName == "core_objects");

            if (coreFile == null)
            {
                coreFile = new ConfigFile<ModifierDefinitionConfig>
                {
                    FileFullPath = "core_objects",
                    IsCore = true,
                    IsOverride = false
                };
                ModDataStorage.Mod.ModifierDefinitions.Add(coreFile); // Если коллекции нет - невозможно, добавить по рекомендации
                Logger.AddDbgLog("Создан core_objects файл для динамических модификаторов.");
            }

            foreach (var mod in docModifiers)
            {
                mod.IsCore = true;
                mod.FileFullPath = "core_objects"; // или оставить оригинальный путь, если важно
                coreFile.Entities.Add(mod);
            }

            modifierFiles.Add(coreFile);
            Logger.AddDbgLog($"Добавлен виртуальный core_objects с {coreFile.Entities.Count} core-определениями модификаторов");
        }
    }
}