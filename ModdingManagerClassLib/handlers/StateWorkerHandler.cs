using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels;
using ModdingManagerModels.Args;
using ModdingManagerModels.Types.ObectCacheData;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Text;
using Path = System.IO.Path;


public class StateWorkerHandler
{
    public List<ProvinceConfig> ComputeProvinceShapes()
    {
        using var mat = Registry.Instance.Map.Bitmap.ToMat();
        if (mat.Empty())
            throw new InvalidOperationException("Не удалось загрузить provinces.bmp");

        Logger.AddLog($"🔍 Начало обработки {Registry.Instance.Map.Provinces.Count} провинций...");

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
                    Logger.AddLog($"⚠️ Провинция {province.Id} не найдена (цвет R:{province.Color.R}, G:{province.Color.G}, B:{province.Color.B})");
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
                    Logger.AddLog($"⚠️ Провинция {province.Id}: контур слишком мал (площадь {moments.M00})");
                    return;
                }

                // 5. Заполняем Shape (берём ВСЕ точки контура)
                province.Shape = new ProvinceShapeArg
                {
                    ContourPoints = mainContour.Select(p => new System.Drawing.Point(p.X, p.Y)).ToArray(),
                    Pos = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00)),
                    FillColor = System.Drawing.Color.FromArgb(255, province.Color.R, province.Color.G, province.Color.B)
                };

                Interlocked.Increment(ref successCount);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"🔥 Ошибка при обработке провинции {province.Id}: {ex.Message}");
            }
        });

        timer.Stop();
        Logger.AddLog("\n====================================");
        Logger.AddLog($"ОБРАБОТКА ЗАВЕРШЕНА за {timer.Elapsed.TotalSeconds:F2} сек");
        Logger.AddLog($"Успешно: {successCount} | Не удалось: {Registry.Instance.Map.Provinces.Count - successCount}");
        Logger.AddLog("====================================\n");

        return Registry.Instance.Map.Provinces;
    }

    public void ChangeState(StateConfig state, string oldName, string newName)
    {
        // Проверяем, что состояние имеет ID и путь к файлу
        if (state.Id == null || string.IsNullOrEmpty(state.FilePath))
            return;

        // Получаем кеш состояний
        var stateCache = Registry.Instance.MapCache.GetStateFiles();

        // Если файл отсутствует в кеше, загружаем его
        if (!stateCache.TryGetValue(state.FilePath, out var cachedFile))
        {
            Registry.Instance.MapCache.AddStateFile(state.FilePath);
            if (!stateCache.TryGetValue(state.FilePath, out cachedFile))
                return; // Файл не удалось загрузить
        }

        // Ищем брекет состояния по ID
        var stateBracket = cachedFile.StateBracket;

        if (stateBracket == null)
            return; // Состояние не найдено в файле
        UpdateStateVariable(stateBracket, "name", state.LocalizationKey, true);
        UpdateStateVariable(stateBracket, "manpower", state.Manpower?.ToString());
        UpdateStateVariable(stateBracket, "state_category", state.Cathegory, true);
        UpdateStateVariable(stateBracket, "local_supplies",
        state.LocalSupply?.ToString(CultureInfo.InvariantCulture));
        UpdateStateName(oldName, state.LocalizationKey, state.Name);

        UpdateBuildings(stateBracket, state.Buildings);
        cachedFile.IsDirty = true;
        Registry.Instance.MapCache.MarkStateFileDirty(state.FilePath);
        Registry.Instance.MapCache.SaveDirtyStateFiles();
    }

    private void UpdateStateName(string oldName, string newName, string newValue)
    {
        string basePath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage);
        string replacePath = Path.Combine(basePath, "replace");
        string fileName = $"state_names_l_{ModManager.CurrentLanguage}.yml";
        string filePath1 = Path.Combine(basePath, fileName);
        string filePath2 = Path.Combine(replacePath, fileName);

        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(replacePath);

        if (!File.Exists(filePath1))
            File.WriteAllText(filePath1, $"l_{ModManager.CurrentLanguage}:\n");
        if (!File.Exists(filePath2))
            File.WriteAllText(filePath2, $"l_{ModManager.CurrentLanguage}:\n");

        try
        {
            using var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var searcher = new FileSearcher
            {
                Files = new List<FileStream> { fileStream1, fileStream2 }
            };

            Var newVar = new Var
            {
                Name = newName,
                Value = newValue,
                AssignSymbol = ':',
                Format = Var.VarFormat.Localisation,
                IsValueQuoted = true
            };

            // Обновляем или добавляем переменную в оба файла
            searcher.SetVar(newVar, 0);
            searcher.SetVar(newVar, 1);

            // Обновляем кэш
            newVar.AddProperty("sourcePath", filePath1);
            Registry.Instance.LocCache.StateLocalisation.RemoveAll(v =>
                v.Value.ToString().Trim('"').Equals(oldName, StringComparison.OrdinalIgnoreCase));
            Registry.Instance.LocCache.StateLocalisation.Add(newVar);
        }
        catch (Exception ex)
        {
            Logger.AddLog($"Error updating state name: {ex.Message}");
        }
    }

    private void UpdateStateVariable(Bracket bracket, string varName, string value, bool isString = false)
    {
        if (value == null) return;

        // Обработка строковых значений (добавляем кавычки)
        if (isString && !value.StartsWith("\"") && !value.EndsWith("\""))
            value = $"\"{value}\"";

        // Ищем существующую переменную
        var existingVarIndex = bracket.SubVars.FindIndex(v => v.Name.Equals(varName, StringComparison.OrdinalIgnoreCase));

        if (existingVarIndex >= 0)
        {
            // Обновляем существующую переменную
            bracket.SubVars[existingVarIndex] = new Var { Name = varName, Value = value };
        }
        else
        {
            // Добавляем новую переменную
            bracket.AddVar(new Var { Name = varName, Value = value });
        }
    }

    private void UpdateBuildings(Bracket stateBracket, List<Var> buildings)
    {
        // Ищем брекет истории
        var historyBracket = stateBracket.SubBrackets
            .FirstOrDefault(b => b.Header.Equals("history", StringComparison.OrdinalIgnoreCase));

        if (historyBracket == null)
        {
            historyBracket = new Bracket { Header = "history" };
            stateBracket.AddSubBracket(historyBracket);
        }

        // Ищем брекет зданий
        var buildingsBracket = historyBracket.SubBrackets
            .FirstOrDefault(b => b.Header.Equals("buildings", StringComparison.OrdinalIgnoreCase));

        // Создаем новый брекет зданий если не найден
        if (buildingsBracket == null)
        {
            buildingsBracket = new Bracket { Header = "buildings" };
            historyBracket.AddSubBracket(buildingsBracket);
        }

        // Полностью заменяем все здания
        buildingsBracket.SubVars.Clear();
        foreach (var building in buildings)
        {
            buildingsBracket.AddVar(building);
        }
    }
    public void ChangeProvince(ProvinceConfig province)
    {
        string modMapDir = Path.Combine(ModManager.ModDirectory, "map");
        string modDefinitions = Path.Combine(modMapDir, "definition.csv");
        string gameDefinitions = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");

        if (!File.Exists(modDefinitions))
        {
            if (!Directory.Exists(modMapDir))
                Directory.CreateDirectory(modMapDir);

            if (!File.Exists(gameDefinitions))
                throw new FileNotFoundException($"Не найден definition.csv ни в моде, ни в игре: {gameDefinitions}");

            File.Copy(gameDefinitions, modDefinitions, true);
            Registry.Instance.MapCache.MapDefinitionCache = new(modDefinitions);
        }

        // Работаем через кеш
        var definitionsContent = Registry.Instance.MapCache.MapDefinitionCache;
        var lines = definitionsContent.DefinitionLines;

        int lineIndex = lines.FindIndex(line =>
        {
            var parts = line.Split(';');
            return parts.Length > 0 && int.TryParse(parts[0], out int id) && id == province.Id;
        });

        string newLine = $"{province.Id};{province.Color.R};{province.Color.G};{province.Color.B};" +
                        $"{province.Type};{(province.IsCoastal ? "true" : "false")};{province.Terrain};{province.ContinentId}";

        if (lineIndex >= 0)
        {
            lines[lineIndex] = newLine;
        }
        else
        {
            lines.Add(newLine);
        }
        definitionsContent.Content = string.Join(Environment.NewLine, lines);
        definitionsContent.IsDirty = true;
        definitionsContent.SaveToFile();


        // 2. Работа с локализацией VictoryPoints (оставляем без изменений, так как это отдельная система)
        string vpKey = $"VICTORY_POINTS_{province.Id}";
        string newLineLoc = $" {vpKey}: \"{province.Name}\"";
        string locFolder = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage);
        string replaceFolder = Path.Combine(locFolder, "replace");

        Directory.CreateDirectory(locFolder);
        Directory.CreateDirectory(replaceFolder);

        string filePath = Path.Combine(locFolder, $"victory_points_l_{ModManager.CurrentLanguage}.yml");
        string replacePath = Path.Combine(replaceFolder, $"victory_points_l_{ModManager.CurrentLanguage}.yml");
        string header = $"﻿l_{ModManager.CurrentLanguage}:\n";

        void EnsureFileHasHeader(string path, string header)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, header, new UTF8Encoding(true));
        }

        void UpdateLineInFile(string path, string key, string line)
        {
            var lines = File.ReadAllLines(path, new UTF8Encoding(true)).ToList();
            bool found = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith($" {key}:"))
                {
                    lines[i] = line;
                    found = true;
                    break;
                }
            }

            if (!found)
                lines.Add(line);

            File.WriteAllLines(path, lines, new UTF8Encoding(true));
        }

        EnsureFileHasHeader(filePath, header);
        EnsureFileHasHeader(replacePath, header);

        UpdateLineInFile(filePath, vpKey, newLineLoc);
        UpdateLineInFile(replacePath, vpKey, newLineLoc);




        // 3. Обновление Victory Points в файлах состояний через кеш
        if (Registry.Instance.MapCache.ProvinceIndex == null)
        {
            Registry.Instance.MapCache.BuildProvinceIndex();
        }

        if (Registry.Instance.MapCache.ProvinceIndex.TryGetValue(province.Id, out var stateInfo))
        {
            var (fileKey, stateBracket) = stateInfo;

            // Находим или создаем history
            var historyBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "history");
            if (historyBracket == null)
            {
                historyBracket = new Bracket { Header = "history" };
                stateBracket.AddSubBracket(historyBracket);
            }

            string vpLine = $"{province.Id} {province.VictoryPoints}";
            var victoryPointsBrackets = historyBracket.SubBrackets.Where(b => b.Header == "victory_points");
            if (victoryPointsBrackets == null)
            {
                var victoryPointsBracket = new Bracket { Header = "victory_points" };
                victoryPointsBracket.AddContent(vpLine);
                historyBracket.AddSubBracket(victoryPointsBracket);
            }
            else
            {
                Bracket currentBracket = historyBracket.SubBrackets
      .FirstOrDefault(b => b.Content.Any(line => line.Contains(province.Id.ToString())));
                if (currentBracket != null)
                {
                    currentBracket.Content.Clear();
                    currentBracket.Content.Add(vpLine);

                }
                else
                {
                    var victoryPointsBracket = new Bracket { Header = "victory_points" };
                    victoryPointsBracket.AddContent(vpLine);
                    historyBracket.AddSubBracket(victoryPointsBracket);
                }
            }

            // Помечаем файл как измененный
            Registry.Instance.MapCache.MarkStateFileDirty(fileKey);

            Registry.Instance.MapCache.SaveDirtyStateFiles();
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
        if (provinceId == null) return;

        var provinceStr = provinceId.ToString();

        if (currentState != null)
        {
            // Обработка текущего штата (удаление провинции)
            var currentContent = File.ReadAllText(currentState.FilePath);
            var currentSearcher = new BracketSearcher { CurrentString = currentContent.ToCharArray() };
            var currentBrackets = currentSearcher.FindBracketsByName("state");

            if (currentBrackets.Count > 0)
            {
                var stateBracket = currentBrackets[0];
                var provincesBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");

                if (provincesBracket != null)
                {
                    provincesBracket.RemoveSubstringFromContentAll(provinceStr);
                    File.WriteAllText(currentState.FilePath, stateBracket.ToString());
                }
            }
        }

        // Обработка целевого штата (добавление провинции)
        var targetContent = File.ReadAllText(targetState.FilePath);
        var targetSearcher = new BracketSearcher { CurrentString = targetContent.ToCharArray() };
        var targetBrackets = targetSearcher.FindBracketsByName("state");

        if (targetBrackets.Count > 0)
        {
            var stateBracket = targetBrackets[0];
            var provincesBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");

            if (provincesBracket == null)
            {
                provincesBracket = new Bracket { Header = "provinces" };
                stateBracket.AddSubBracket(provincesBracket);
            }

            provincesBracket.AddContent(provinceStr);
            File.WriteAllText(targetState.FilePath, stateBracket.ToString());
        }
    }

    public void MoveStateToCountry(StateConfig state, string countryTag)
    {
        var content = File.ReadAllText(state.FilePath);
        var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
        var stateBrackets = searcher.FindBracketsByName("state");

        if (stateBrackets.Count > 0)
        {
            var stateBracket = stateBrackets[0];
            var ownerVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "owner");

            if (ownerVar != null)
            {
                ownerVar.Value = countryTag;
            }
            else
            {
                stateBracket.AddVar(new Var { Name = "owner", Value = countryTag });
            }

            File.WriteAllText(state.FilePath, stateBracket.ToString());
        }
    }

    public void MoveProvinceToStrategicRegion(int? provinceId, StrategicRegionConfig? currentRegion, StrategicRegionConfig targetRegion)
    {
        if (provinceId == null) return;

        var provinceStr = provinceId.ToString();

        if (currentRegion != null)
        {
            // Обработка текущего региона (удаление провинции)
            var currentContent = File.ReadAllText(currentRegion.FilePath);
            var currentSearcher = new BracketSearcher { CurrentString = currentContent.ToCharArray() };
            var currentBrackets = currentSearcher.FindBracketsByName("strategic_region");

            if (currentBrackets.Count > 0)
            {
                var regionBracket = currentBrackets[0];
                var provincesBracket = regionBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");

                if (provincesBracket != null)
                {
                    provincesBracket.RemoveSubstringFromContentAll(provinceStr);
                    File.WriteAllText(currentRegion.FilePath, regionBracket.ToString());
                }
            }
        }

        // Обработка целевого региона (добавление провинции)
        var targetContent = File.ReadAllText(targetRegion.FilePath);
        var targetSearcher = new BracketSearcher { CurrentString = targetContent.ToCharArray() };
        var targetBrackets = targetSearcher.FindBracketsByName("strategic_region");

        if (targetBrackets.Count > 0)
        {
            var regionBracket = targetBrackets[0];
            var provincesBracket = regionBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");

            if (provincesBracket == null)
            {
                provincesBracket = new Bracket { Header = "provinces" };
                regionBracket.AddSubBracket(provincesBracket);
            }

            provincesBracket.AddContent(provinceStr);
            File.WriteAllText(targetRegion.FilePath, regionBracket.ToString());
        }
    }
}