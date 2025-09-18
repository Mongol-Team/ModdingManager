using ModdingManagerClassLib.Extentions;
using ModdingManager.classes.utils.fonts;
using ModdingManager.managers.@base;
using ModdingManagerModels.SuperEventModels;
using ModdingManagerModels.Types;
using System.Diagnostics;
using System.Drawing;                             
using System.Text;
using System.Windows;                             
using ModdingManagerClassLib.Debugging;
public class SuperEventHandler
{
    public SupereventConfig CurrentConfig { get; set; }

    // ==== AUDIO ====

    public void SaveSupereventAudioFile()
    {
        if (CurrentConfig?.SoundPath == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) return;

        try
        {
            var sourceUri = new Uri(CurrentConfig.SoundPath, UriKind.Absolute);
            if (!sourceUri.IsFile) return;

            string sourcePath = sourceUri.LocalPath;
            string destinationDirectory = Path.Combine(ModManager.ModDirectory, "sound", "customsound");
            Directory.CreateDirectory(destinationDirectory);

            string destinationPath = Path.Combine(destinationDirectory, $"{CurrentConfig.Id.ToString()}_sound.wav");
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving superevent audio: {ex.Message}");
        }
    }

    public void CreateCustomAssetsFiles()
    {
        if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) return;

        string soundDirectory = Path.Combine(ModManager.ModDirectory, "sound");
        Directory.CreateDirectory(soundDirectory);
        Directory.CreateDirectory(Path.Combine(soundDirectory, "customsound"));

        UpdateSupereventsAsset(soundDirectory);
        CreateSoundAsset(soundDirectory);
        CreateSoundEffectAsset(soundDirectory);
    }

    private void UpdateSupereventsAsset(string soundDirectory)
    {
        string assetPath = Path.Combine(soundDirectory, "superevents.asset");
        List<string> soundEffects = new();

        if (File.Exists(assetPath))
        {
            string[] lines = File.ReadAllLines(assetPath);
            bool inSection = false;
            foreach (string line in lines)
            {
                var t = line.Trim();
                if (t.StartsWith("soundeffects = {")) { inSection = true; continue; }
                if (inSection)
                {
                    if (t.StartsWith("}")) { inSection = false; continue; }
                    string cleaned = line.Split('/')[0].Trim();
                    if (!string.IsNullOrEmpty(cleaned)) soundEffects.Add(cleaned);
                }
            }
        }

        string newEffect = $"{CurrentConfig.Id}_soundeffect";
        if (!soundEffects.Contains(newEffect)) soundEffects.Add(newEffect);

        using var w = new StreamWriter(assetPath);
        w.WriteLine("category = {");
        w.WriteLine("\tname = \"SuperEventsSoundEffects\"");
        w.WriteLine("\tsoundeffects = {");
        foreach (var e in soundEffects) w.WriteLine($"\t\t{e}");
        w.WriteLine("\t}");
        w.WriteLine("\tcompressor = {");
        w.WriteLine("\t\tenabled = yes");
        w.WriteLine("\t\tpregain = 3.0");
        w.WriteLine("\t\tpostgain = 0.0");
        w.WriteLine("\t\tratio = 10.0");
        w.WriteLine("\t\tthreshold = -15.0");
        w.WriteLine("\t\tattacktime = 0.030");
        w.WriteLine("\t\treleasetime = 1.2");
        w.WriteLine("\t}");
        w.WriteLine("}");
    }

    private void CreateSoundAsset(string soundDirectory)
    {
        using var w = new StreamWriter(Path.Combine(soundDirectory, $"{CurrentConfig.Id}_sound.asset"));
        w.WriteLine("sound = { ");
        w.WriteLine($"\tname = \"{CurrentConfig.Id}_sound\"");
        w.WriteLine($"\tfile = \"customsound/{CurrentConfig.Id}_sound.wav\"");
        w.WriteLine("\talways_load = no");
        w.WriteLine("}");
    }

    private void CreateSoundEffectAsset(string soundDirectory)
    {
        using var w = new StreamWriter(Path.Combine(soundDirectory, $"{CurrentConfig.Id}_soundeffect.asset"));
        w.WriteLine("soundeffect = { ");
        w.WriteLine($"\tname = {CurrentConfig.Id}_soundeffect");
        w.WriteLine("\tloop = no");
        w.WriteLine("\tsounds = {");
        w.WriteLine($"\t\tsound = {CurrentConfig.Id}_sound");
        w.WriteLine("\t}");
        w.WriteLine("\tis3d = no");
        w.WriteLine("\tmax_audible = 1");
        w.WriteLine("\tmax_audible_behaviour = fail");
        w.WriteLine("\tvolume = 1.5");
        w.WriteLine("}");
    }

    // ==== IMAGES (из GuiDocument, через маппинг Sprite -> filePath) ====

    /// <summary>
    /// Сохраняет файлы кнопок в gfx/superevent/button/{id}_{button.fontSign}_bg.dds,
    /// если в Config.SpriteSources есть путь для соответствующего sprite (quadTextureSprite или паттерн).
    /// </summary>
    public void HandleButtonsImage()
    {
        if (CurrentConfig?.Gui == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) { MessageBox.Show("GUI not loaded!"); return; }
        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win == null) { MessageBox.Show("Gui window not found!"); return; }

        string dir = Path.Combine(ModManager.ModDirectory, "gfx", "superevent", "button");
        Directory.CreateDirectory(dir);

        for (int i = 0; i < win.Buttons.Count; i++)
        {
            var btn = win.Buttons[i];
            // имя файла берём из имени кнопки (или OptionA/B/C по индексу)
            string optionName = !string.IsNullOrWhiteSpace(btn.Name) ? btn.Name : $"Option{(char)('A' + i)}";
            optionName = optionName.Replace(' ', '_').ToLowerInvariant();

            string spriteName = btn.QuadTextureSprite ?? $"GFX_{CurrentConfig.Id}_option_{(char)('A' + i)}_bg";
            if (TryGetSpriteSource(spriteName, out string sourcePath))
            {
                string outPath = Path.Combine(dir, $"{CurrentConfig.Id}_{optionName}_bg.dds");
                SaveAsDDS(sourcePath, outPath);
            }
        }
    }

    /// <summary>
    /// Сохраняет картинку и рамку суперивента:
    ///  - image → gfx/superevent_pictures/superevent_image_{id}.tga
    ///  - background → gfx/interface/superevent/superevent_frame_{id}.dds
    /// </summary>
    public void SaveSupereventImages()
    {
        if (CurrentConfig?.Gui == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) return;
        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win == null) return;

        var img = win.Icons.FirstOrDefault(i => (i.Name ?? "").Equals("image", StringComparison.OrdinalIgnoreCase));
        var bg = win.Icons.FirstOrDefault(i => (i.Name ?? "").Equals("background", StringComparison.OrdinalIgnoreCase));

        if (img != null && TryGetSpriteSource(img.SpriteType, out string imgSrc))
        {
            string imageDir = Path.Combine(ModManager.ModDirectory, "gfx", "superevent_pictures");
            Directory.CreateDirectory(imageDir);
            string outTga = Path.Combine(imageDir, $"superevent_image_{CurrentConfig.Id}.tga");
            SaveAsTGA(imgSrc, outTga);
        }

        if (bg != null && TryGetSpriteSource(bg.SpriteType, out string bgSrc))
        {
            string frameDir = Path.Combine(ModManager.ModDirectory, "gfx", "interface", "superevent");
            Directory.CreateDirectory(frameDir);
            string outDds = Path.Combine(frameDir, $"superevent_frame_{CurrentConfig.Id}.dds");
            SaveAsDDS(bgSrc, outDds);
        }
    }

    private bool TryGetSpriteSource(string spriteTypeName, out string path)
    {
        path = null!;
        // ожидается мапа Config.SpriteSources[spriteName] = "C:\...\img.png"
        if (CurrentConfig.SpriteSources != null &&
            CurrentConfig.SpriteSources.TryGetValue(spriteTypeName, out var p) &&
            File.Exists(p))
        {
            path = p; return true;
        }
        return false;
    }

    private void SaveAsDDS(string sourceImagePath, string outDdsPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outDdsPath)!);
        using var img = Image.FromFile(sourceImagePath);
        // предполагается, что у тебя есть расширение SaveAsDDS(System.Drawing.Image, ...)
        img.SaveAsDDS(Path.GetDirectoryName(outDdsPath)!, Path.GetFileNameWithoutExtension(outDdsPath),
                      img.Width, img.Height);
    }

    private void SaveAsTGA(string sourceImagePath, string outTgaPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outTgaPath)!);
        using var img = Image.FromFile(sourceImagePath);
        // предполагается расширение ConvertToImageSharp().SaveAsTGA(...)
        img.ConvertToImageSharp().SaveAsTGA(outTgaPath);
    }

    // ==== .GFX из GuiDocument ====

    public void HandleGFXFile()
    {
        if (CurrentConfig?.Gui == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) return;

        string guiDir = Path.Combine(ModManager.ModDirectory, "interface");
        Directory.CreateDirectory(guiDir);
        string filePath = Path.Combine(guiDir, $"SUPEREVENT_{CurrentConfig.Id}_window.gfx");

        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win == null) return;

        using var w = new StreamWriter(filePath);
        w.WriteLine("spriteTypes = {");

        // Иконки (image/background) — пишем как есть по spriteType
        foreach (var ic in win.Icons)
        {
            // по конвенции кладём:
            //  - image → gfx/superevent_pictures/superevent_image_{id}.tga
            //  - background → gfx/interface/superevent/superevent_frame_{id}.dds
            string texture = ic.Name?.Equals("image", StringComparison.OrdinalIgnoreCase) == true
                ? $"gfx\\\\superevent_pictures\\\\superevent_image_{CurrentConfig.Id}.tga"
                : ic.Name?.Equals("background", StringComparison.OrdinalIgnoreCase) == true
                    ? $"gfx\\\\interface\\\\superevent\\\\superevent_frame_{CurrentConfig.Id}.dds"
                    : $"gfx\\\\interface\\\\{ic.SpriteType}.dds"; // fallback

            w.WriteLine("\tspriteType = {");
            w.WriteLine($"\t\tname = \"{ic.SpriteType}\"");
            w.WriteLine($"\t\ttextureFile = \"{texture}\"");
            w.WriteLine("\t}");
        }

        // Кнопки — если у кнопки есть свой sprite (quadTextureSprite)
        for (int i = 0; i < win.Buttons.Count; i++)
        {
            var btn = win.Buttons[i];
            string spriteName = btn.QuadTextureSprite ?? $"GFX_{CurrentConfig.Id}_option_{(char)('A' + i)}_bg";
            string optionName = !string.IsNullOrWhiteSpace(btn.Name) ? btn.Name : $"Option{(char)('A' + i)}";
            optionName = optionName.Replace(' ', '_').ToLowerInvariant();

            w.WriteLine("\tspriteType = {");
            w.WriteLine($"\t\tname = \"{spriteName}\"");
            w.WriteLine($"\t\ttextureFile = \"gfx\\\\superevent\\\\button\\\\{CurrentConfig.Id}_{optionName}_bg.dds\"");
            w.WriteLine("\t\tnoOfFrames = 1");
            w.WriteLine("\t\teffectFile = \"gfx/FX/buttonstate.lua\"");
            w.WriteLine("\t}");
        }

        w.WriteLine("}");
    }

    // ==== .GUI из GuiDocument ====

    public void HandleGUIFile()
    {
        if (CurrentConfig?.Gui == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString())) return;

        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win == null) return;

        string guiDirectory = Path.Combine(ModManager.ModDirectory, "interface");
        Directory.CreateDirectory(guiDirectory);
        string filePath = Path.Combine(guiDirectory, $"SUPEREVENT_{CurrentConfig.Id}_window.gui");

        using var w = new StreamWriter(filePath);
        w.WriteLine("guiTypes = {");
        w.WriteLine("\tcontainerWindowType = {");
        w.WriteLine($"\t\tname = \"SUPEREVENT_{CurrentConfig.Id}_window\"");
        w.WriteLine($"\t\tsize = {{ width = {win.Size.Width} height = {win.Size.Height} }}");
        w.WriteLine("\t\tposition = { x=0 y=0 }");
        w.WriteLine($"\t\tOrientation = {win.Orientation ?? "center"}");
        w.WriteLine($"\t\tOrigo = {win.Origo ?? "center"}");
        w.WriteLine($"\t\tclipping = {(win.Clipping == true ? "yes" : "no")}");
        w.WriteLine($"\t\tshow_sound = {CurrentConfig.Id}_soundeffect");
        w.WriteLine();

        // Icons
        foreach (var ic in win.Icons)
        {
            w.WriteLine("\t\ticonType = {");
            w.WriteLine($"\t\t\tname = \"{ic.Name}\"");
            w.WriteLine($"\t\t\tspriteType = \"{ic.SpriteType}\"");
            w.WriteLine($"\t\t\tposition = {{ x = {ic.Position.X} y = {ic.Position.Y} }}");
            if (!string.IsNullOrEmpty(ic.Orientation)) w.WriteLine($"\t\t\tOrientation = {ic.Orientation}");
            if (ic.AlwaysTransparent == true) w.WriteLine("\t\t\talwaystransparent = yes");
            w.WriteLine("\t\t}");
            w.WriteLine();
        }

        // Texts
        foreach (var t in win.Texts)
        {
            w.WriteLine("\t\tinstantTextBoxType = {");
            w.WriteLine($"\t\t\tname = \"{t.Name}\"");
            w.WriteLine($"\t\t\tposition = {{ x = {t.Position.X} y = {t.Position.Y} }}");
            w.WriteLine($"\t\t\tfont = \"{t.Font}\"");
            if (t.BorderSize.HasValue) w.WriteLine($"\t\t\tborderSize = {{x = {t.BorderSize.Value.X} y = {t.BorderSize.Value.Y}}}");
            w.WriteLine($"\t\t\ttext = \"{t.Text}\"");
            if (t.MaxWidth.HasValue) w.WriteLine($"\t\t\tmaxWidth = {t.MaxWidth.Value}");
            if (t.MaxHeight.HasValue) w.WriteLine($"\t\t\tmaxHeight = {t.MaxHeight.Value}");
            if (t.FixedSize == true) w.WriteLine("\t\t\tfixedsize = yes");
            if (!string.IsNullOrEmpty(t.Orientation)) w.WriteLine($"\t\t\tOrientation = {t.Orientation}");
            if (!string.IsNullOrEmpty(t.Format)) w.WriteLine($"\t\t\tformat = {t.Format}");
            w.WriteLine("\t\t}");
            w.WriteLine();
        }

        // Buttons
        for (int i = 0; i < win.Buttons.Count; i++)
        {
            var b = win.Buttons[i];
            char ch = (char)('A' + i);

            w.WriteLine("\t\tbuttonType = {");
            w.WriteLine($"\t\t\tname = \"{b.Name}\"");
            // если в модели уже хранится конечный ключ, используем его; иначе соберём как в твоём генераторе
            string buttonKey = !string.IsNullOrWhiteSpace(b.Text) ? b.Text : $"SUPEREVENT_{CurrentConfig.Id}_OPTION_{ch}";
            w.WriteLine($"\t\t\ttext = \"{buttonKey}\"");
            if (!string.IsNullOrWhiteSpace(b.Shortcut)) w.WriteLine($"\t\t\tshortcut = \"{b.Shortcut}\"");
            else if (i == 0) w.WriteLine("\t\t\tshortcut = \"ESCAPE\"");
            w.WriteLine($"\t\t\tposition = {{ x = {b.Position.X} y = {b.Position.Y} }}");
            if (!string.IsNullOrWhiteSpace(b.QuadTextureSprite))
                w.WriteLine($"\t\t\tquadTextureSprite =\"{b.QuadTextureSprite}\"");
            else
                w.WriteLine($"\t\t\tquadTextureSprite =\"GFX_button_221x34\"");
            if (!string.IsNullOrWhiteSpace(b.Font.Family))
                w.WriteLine($"\t\t\tbuttonFont = \"{b.Font.Family}\"");
            if (!string.IsNullOrWhiteSpace(b.Orientation))
                w.WriteLine($"\t\t\tOrientation = {b.Orientation}");
            w.WriteLine("\t\t}");
            w.WriteLine();
        }

        w.WriteLine("\t}");
        w.WriteLine("}");
    }

    // ==== LOCALIZATION (.yml) из GuiDocument ====

    public void HandleLocalizationFiles()
    {
        if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString()) || CurrentConfig.Gui == null) return;

        try
        {
            HandleLocalizationFile("russian", "l_russian:", CurrentConfig.Header ?? "", CurrentConfig.Description ?? "");
            HandleLocalizationFile("english", "l_english:", "", "");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при создании файлов локализации: {ex.Message}");
        }
    }

    private void HandleLocalizationFile(string language, string header, string titleValue, string descValue)
    {
        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win == null) return;

        string locDirectory = Path.Combine(ModManager.ModDirectory, "localisation", language);
        Directory.CreateDirectory(locDirectory);

        string filePath = Path.Combine(locDirectory, $"superevents_l_{language}.yml");
        List<string> lines = File.Exists(filePath)
            ? File.ReadAllLines(filePath, Encoding.UTF8).ToList()
            : new List<string>();

        if (lines.Count == 0 || !lines[0].StartsWith(header))
            lines.Insert(0, header);

        string titleKey = $" SUPEREVENT_{CurrentConfig.Id}_TITLE";
        string descKey = $" SUPEREVENT_{CurrentConfig.Id}_DESC";

        // Удаляем старые записи Title/Desc
        lines = lines.Where(line => !line.StartsWith(titleKey + ":") && !line.StartsWith(descKey + ":")).ToList();

        // Title/Desc
        lines.Add($" {titleKey}: \"§M{EscapeYamlString(titleValue)}\"");
        lines.Add($" {descKey}: \"§M{EscapeYamlString(descValue)}\"");

        // Опции по порядку кнопок
        for (int i = 0; i < win.Buttons.Count; i++)
        {
            char ch = (char)('A' + i);
            string key = $" SUPEREVENT_{CurrentConfig.Id}_OPTION_{ch}";
            // Текст опции берём из Config.OptionTexts[ch], если есть; иначе — placeholder по имени кнопки
            string optionText =
                (CurrentConfig.OptionTexts != null && CurrentConfig.OptionTexts.TryGetValue(ch, out var txt) ? txt :
                (!string.IsNullOrWhiteSpace(win.Buttons[i].Name) ? win.Buttons[i].Name : $"Option {ch}"));

            // удаляем старую строку ключа
            lines = lines.Where(line => !line.StartsWith(key + ":")).ToList();
            lines.Add($" {key}: \"§M{EscapeYamlString(optionText)}\"");
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    private string EscapeYamlString(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        input = input.Replace("\r", "").Replace("\n", " ").Trim();
        return input.Replace("\"", "\\\"");
    }

    // ==== SCRIPTED GUI (.txt) из GuiDocument ====

    public void HandleScriptedGuiFile()
    {
        if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id.ToString()) || CurrentConfig.Gui == null) return;

        string dir = Path.Combine(ModManager.ModDirectory, "common", "scripted_guis");
        Directory.CreateDirectory(dir);
        string filePath = Path.Combine(dir, $"SUPEREVENT_{CurrentConfig.Id}_scripted_gui.txt");

        using var w = new StreamWriter(filePath);
        w.WriteLine("scripted_gui = {");
        w.WriteLine();
        w.WriteLine($"\tSUPEREVENT_{CurrentConfig.Id}_window = {{ ");
        w.WriteLine("\t\tcontext_type = player_context");
        w.WriteLine($"\t\twindow_name = \"SUPEREVENT_{CurrentConfig.Id}_window\"");
        w.WriteLine();
        w.WriteLine("\t\tvisible = {");
        w.WriteLine($"\t\t\thas_country_flag = superevent_{CurrentConfig.Id}_flag");
        w.WriteLine("\t\t}");
        w.WriteLine();
        w.WriteLine("\t\teffects = {");

        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        int count = win?.Buttons.Count ?? 0;
        for (int i = 0; i < count; i++)
        {
            char ch = (char)('A' + i);
            w.WriteLine($"\t\t\tOption{ch}_click = {{");
            w.WriteLine($"\t\t\t\tclr_country_flag = superevent_{CurrentConfig.Id}_flag");
            w.WriteLine("\t\t\t}");
        }

        w.WriteLine("\t\t}");
        w.WriteLine("\t}");
        w.WriteLine("}");
    }

    // ==== FONTS ====
    // Из GuiDocument достаём имена шрифтов (TextBox.Font + Button.ButtonFont) и генерим ассеты.
    // Если у тебя остаётся свой FontSignature с цветами — подставь нужные конструкторы.

    public void HandleFontFiles()
    {
        string fontsDirectory = Path.Combine(ModManager.ModDirectory, "gfx", "fonts");
        Directory.CreateDirectory(fontsDirectory);

        var uniqueFonts = FontManager.CollectUniqueFonts(CurrentConfig.Gui);


        try
        {
            foreach (var fontSignature in uniqueFonts)
            {
                string fontFileName = FontManager.GenerateFontName(fontSignature);
                string fntPath = Path.Combine(fontsDirectory, $"{fontFileName}.fnt");
                string ddsPath = Path.Combine(fontsDirectory, $"{fontFileName}.dds");
                string tgaPath = Path.Combine(fontsDirectory, $"{fontFileName}.tga");

                if (File.Exists(fntPath) && (File.Exists(ddsPath) || File.Exists(tgaPath))) continue;

                FontManager.HandleFontFolderWithChecks(fontsDirectory, fontSignature);
            }
        }
        catch (OperationCanceledException)
        {

            Logger.AddLog("[WPF EXEPTION]: Операция отменена пользователем или из-за ошибок покрытия шрифта.");
        }
        catch (Exception ex)
        {
           
            MessageBox.Show($"Не удалось завершить операцию:\n{ex.Message}",
                        "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Logger.AddLog($"[WPF EXEPTION]: Не удалось завершить операцию:{ex.Message}");
            
        }
    }

    public void HandleFontDefineFiles()
    {
        if (CurrentConfig?.Gui == null) return;

        string interfaceDirectory = Path.Combine(ModManager.ModDirectory, "interface");
        Directory.CreateDirectory(interfaceDirectory);
        string filePath = Path.Combine(interfaceDirectory, "font_definitions.gfx");

        HashSet<FontSignature> uniqueFonts = CollectFontSignatures();

        using var w = new StreamWriter(filePath);
        w.WriteLine("bitmapfonts = {");

        foreach (var fontSign in uniqueFonts)
        {
            // если нужны цвета, возьми их из своей модели/настроек; здесь фиксируем белый
            w.WriteLine("\tbitmapfont = {");
            w.WriteLine($"\t\tname = \"{FontManager.GenerateFontName(fontSign)}\"");
            w.WriteLine("\t\tfontfiles = {");
            w.WriteLine($"\t\t\t\"gfx/fonts/{FontManager.GenerateFontName(fontSign)}\"");
            w.WriteLine("\t\t}");
            w.WriteLine("\t\tcolor = 0xffffffff");
            w.WriteLine("\t\ttextcolors = {");
            w.WriteLine("\t\t\tM = { 255 255 255 }");
            w.WriteLine("\t\t}");
            w.WriteLine("\t}");
            w.WriteLine();
        }

        w.WriteLine("}");
    }

    private HashSet<FontSignature> CollectFontSignatures()
    {
        HashSet<FontSignature> set = new HashSet<FontSignature>();
        var win = CurrentConfig.Gui.Containers.FirstOrDefault();
        if (win != null)
        {
            foreach (var t in win.Texts)
                if (!string.IsNullOrWhiteSpace(t.Font.Family)) set.Add(t.Font);

            foreach (var b in win.Buttons)
                if (!string.IsNullOrWhiteSpace(b.Font.Family)) set.Add(b.Font);
        }
        return set;
    }
}
