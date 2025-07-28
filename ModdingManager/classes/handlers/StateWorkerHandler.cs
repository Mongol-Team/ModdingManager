using ModdingManager.classes.args;
using ModdingManager.classes.configs;
using ModdingManager.classes.utils;
using ModdingManager.classes.utils.search;
using ModdingManager.classes.utils.types;
using ModdingManager.managers.utils;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.IO.Path;


public class StateWorkerHandler
{
    public List<ProvinceConfig> ComputeProvinceShapes()
    {
        using var mat = Registry.Instance.Map.Bitmap.ToMat();
        if (mat.Empty())
            throw new InvalidOperationException("Не удалось загрузить provinces.bmp");

        Debugger.Instance.LogMessage($"🔍 Начало обработки {Registry.Instance.Map.Provinces.Count} провинций...");

        int successCount = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();

        int maxThreads = Math.Max(1, Environment.ProcessorCount / 3);
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };

        Parallel.ForEach(Registry.Instance.Map.Provinces, parallelOptions, province =>
        {
            try
            {
                using var mask = new Mat();
                Cv2.InRange(mat,
                    new Scalar(province.Color.B, province.Color.G, province.Color.R),
                    new Scalar(province.Color.B, province.Color.G, province.Color.R),
                    mask);

                int pixelCount = Cv2.CountNonZero(mask);
                if (pixelCount == 0)
                {
                    Debugger.Instance.LogMessage($"⚠️ Провинция {province.Id} не найдена (цвет R:{province.Color.R}, G:{province.Color.G}, B:{province.Color.B})");
                    return;
                }

                Cv2.FindContours(mask, out var contours, out _,
         RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                if (contours.Length == 0)
                    return;

                var mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();

                double area = Cv2.ContourArea(mainContour);
                double perimeter = Cv2.ArcLength(mainContour, true);

                bool isSimple = mainContour.Length < 50 ||
                                (4 * Math.PI * area / (perimeter * perimeter) > 0.5);

                if (!isSimple)
                {
                    
                    Cv2.FindContours(mask, out contours, out _,
                        RetrievalModes.External, ContourApproximationModes.ApproxNone);

                    mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                }

                var moments = Cv2.Moments(mainContour);
                if (moments.M00 <= 0.5)
                {
                    Debugger.Instance.LogMessage($"⚠️ Провинция {province.Id}: контур слишком мал (площадь {moments.M00})");
                    return;
                }

                // 5. Заполняем Shape (берём ВСЕ точки контура)
                province.Shape = new ProvinceShapeArg
                {
                    ContourPoints = mainContour.Select(p => new System.Windows.Point(p.X, p.Y)).ToArray(),
                    Pos = new System.Windows.Point(moments.M10 / moments.M00, moments.M01 / moments.M00),
                    FillColor = System.Windows.Media.Color.FromArgb(255, province.Color.R, province.Color.G, province.Color.B)
                };

                Interlocked.Increment(ref successCount);
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"🔥 Ошибка при обработке провинции {province.Id}: {ex.Message}");
            }
        });

        // ==============================
        // Итог
        // ==============================
        timer.Stop();
        Debugger.Instance.LogMessage("\n====================================");
        Debugger.Instance.LogMessage($"ОБРАБОТКА ЗАВЕРШЕНА за {timer.Elapsed.TotalSeconds:F2} сек");
        Debugger.Instance.LogMessage($"Успешно: {successCount} | Не удалось: {Registry.Instance.Map.Provinces.Count - successCount}");
        Debugger.Instance.LogMessage("====================================\n");

        return Registry.Instance.Map.Provinces;
    }

    public void ChangeState(StateConfig state)
    {
        
    }
    public void ChangeProvince(ProvinceConfig province)
    {
        // Пути
        string modMapDir = Path.Combine(ModManager.Directory, "map");
        string modDefinitions = Path.Combine(modMapDir, "definition.csv");
        string gameDefinitions = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");

        // 1. Проверяем наличие definitions.csv в моде, если нет - копируем из игры
        if (!File.Exists(modDefinitions))
        {
            if (!Directory.Exists(modMapDir))
                Directory.CreateDirectory(modMapDir);

            if (!File.Exists(gameDefinitions))
                throw new FileNotFoundException($"Не найден definition.csv ни в моде, ни в игре: {gameDefinitions}");

            File.Copy(gameDefinitions, modDefinitions, true);
        }

        // 2. Читаем все строки definitions.csv
        var lines = File.ReadAllLines(modDefinitions, Encoding.UTF8).ToList();

        // 3. Ищем строку с Id провинции
        int lineIndex = lines.FindIndex(line =>
        {
            var parts = line.Split(';');
            return parts.Length > 0 && int.TryParse(parts[0], out int id) && id == province.Id;
        });

        // 4. Формируем новую строку
        string newLine = $"{province.Id};{province.Color.R};{province.Color.G};{province.Color.B};" +
                         $"{province.Type};{(province.IsCoastal ? 1 : 0)};{province.Terrain};{province.ContinentId}";

        if (lineIndex >= 0)
        {
            lines[lineIndex] = newLine;
        }
        else
        {
            // Если нет такой строки - добавляем
            lines.Add(newLine);
        }

        // 5. Сохраняем обновленный definitions.csv
        File.WriteAllLines(modDefinitions, lines, Encoding.UTF8);

        // 6. Работа с локализацией VictoryPoints
        string vpKey = $"VICTORY_POINTS_{province.Id}";
        var victoryPoints = Registry.Instance.Cache.VictoryPointsLocalisation;
        var vpVar = victoryPoints.FirstOrDefault(v => v.Name.Equals(vpKey, StringComparison.OrdinalIgnoreCase));

        if (vpVar != null)
        {
            // Есть существующая локализация - обновляем её через VarSearcher
            string sourcePath = vpVar.GetProperty("sourcePath") as string;

            if (sourcePath != null && File.Exists(sourcePath))
            {
                var vpLines = File.ReadAllLines(sourcePath, Encoding.UTF8);
                vpVar.Value = province.Name; // обновляем имя
                var updated = VarSearcher.SetSourceValue(vpLines, vpVar, ":");
                if (updated != null)
                    File.WriteAllLines(sourcePath, updated, new UTF8Encoding(true));
            }
        }
        else
        {
            // Нет локализации - создаем новый файл victory_points_l_{lang}.yml
            string locFolder = Path.Combine(ModManager.Directory, "localisation", ModManager.CurrentLanguage);
            string replaceFolder = Path.Combine(locFolder, "replace");

            if (!Directory.Exists(locFolder))
                Directory.CreateDirectory(locFolder);
            if (!Directory.Exists(replaceFolder))
                Directory.CreateDirectory(replaceFolder);

            string filePath = Path.Combine(locFolder, $"victory_points_l_{ModManager.CurrentLanguage}.yml");

            // Если файла нет - создаем с заголовком
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $"﻿l_{ModManager.CurrentLanguage}:\n", new UTF8Encoding(true));
            }

            // Добавляем новую запись
            string newLineLoc = $" {vpKey}: \"{province.Name}\"";
            File.AppendAllText(filePath, newLineLoc + Environment.NewLine, new UTF8Encoding(true));

            // Добавляем в кеш VictoryPointsLocalisation
            var newVar = new Var { Name = vpKey, Value = province.Name };
            newVar.AddProperty("sourcePath", filePath);
            victoryPoints.Add(newVar);
        }
    }
    public void ChangeStrategicRegion(StrategicRegionConfig region)
    {
        
    }
    public void ChangeCountry(CountryOnMapConfig country)
    {
        
    }
    public void MoveProvinceToState(int? provinceId, StateConfig? currentState, StateConfig targetState)
    {
        var searcher = new BracketSearcher();

        if (currentState != null)
        {
            string currentText = File.ReadAllText(currentState.FilePath);
            searcher.CurrentString = currentText.ToCharArray();
            searcher.RemoveBracketContentFromBlock("provinces", provinceId.ToString(), " ");

            string updatedCurrent = new string(searcher.CurrentString);
            File.WriteAllText(currentState.FilePath, updatedCurrent);
        }

        string targetText = File.ReadAllText(targetState.FilePath);
        searcher.CurrentString = targetText.ToCharArray();
        searcher.AddBracketContentToBlock("provinces", provinceId.ToString(), " ");

        string updatedTarget = new string(searcher.CurrentString);
        File.WriteAllText(targetState.FilePath, updatedTarget);
    }

    public void MoveStateToCountry(StateConfig state, string countryTag)
    {
        string fileContent = File.ReadAllText(state.FilePath);
        var lines = fileContent.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("owner"))
            {
                int equalIndex = line.IndexOf('=');
                if (equalIndex != -1)
                {
                    lines[i] = $"owner = {countryTag}";
                    break;
                }
            }
        }
        File.WriteAllText(state.FilePath, string.Join("\n", lines));
    }
    public void MoveProvinceToStrategicRegion(int? provinceId, StrategicRegionConfig? currentRegion, StrategicRegionConfig targetRegion)
    {
        var searcher = new BracketSearcher();

        if (currentRegion != null)
        {
            string currentText = File.ReadAllText(currentRegion.FilePath);
            searcher.CurrentString = currentText.ToCharArray();
            searcher.RemoveBracketContentFromBlock("provinces", provinceId.ToString(), " ");

            string updatedCurrent = new string(searcher.CurrentString);
            File.WriteAllText(currentRegion.FilePath, updatedCurrent);
        }

        string targetText = File.ReadAllText(targetRegion.FilePath);
        searcher.CurrentString = targetText.ToCharArray();
        searcher.AddBracketContentToBlock("provinces", provinceId.ToString(), " ");

        string updatedTarget = new string(searcher.CurrentString);
        File.WriteAllText(targetRegion.FilePath, updatedTarget);
    }


}