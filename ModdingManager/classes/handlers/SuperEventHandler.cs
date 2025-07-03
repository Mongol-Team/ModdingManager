using ModdingManager.classes.extentions;
using ModdingManager.classes.gfx;
using ModdingManager.classes.utils.fonts;
using ModdingManager.configs;
using ModdingManager.managers.utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
namespace ModdingManager.classes.handlers
{
    public class SuperEventHandler
    {
        public SupereventConfig CurrentConfig {  get; set; }
        public void SaveSupereventAudioFile()
        {
            if (CurrentConfig == null || CurrentConfig.SoundPath == null || string.IsNullOrEmpty(CurrentConfig.Id))
            {
                return; 
            }

            try
            {
                Uri sourceUri = new Uri(CurrentConfig.SoundPath, UriKind.Absolute);
                if (sourceUri == null || !sourceUri.IsFile)
                {
                    return; 
                }

                string sourcePath = sourceUri.LocalPath;
                string destinationDirectory = Path.Combine(ModManager.Directory, "sound", "customsound");

                Directory.CreateDirectory(destinationDirectory);

                string destinationFileName = $"{CurrentConfig.Id}_sound.wav";
                string destinationPath = Path.Combine(destinationDirectory, destinationFileName);

                File.Copy(sourcePath, destinationPath, overwrite: true);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving superevent audio: {ex.Message}");
            }
        }
        public void HandleButtonsImage()
        {
            var mainCanvas = CurrentConfig.EventConstructor;
            if (mainCanvas == null)
            {
                System.Windows.MessageBox.Show("Main canvas not found!");
                return;
            }

            var buttonCanvases = mainCanvas.FindElementsOfType<Canvas>();

            foreach (var buttonCanvas in buttonCanvases)
            {
                var buttonImage = buttonCanvas.FindElementsOfType<System.Windows.Controls.Image>().FirstOrDefault();
                if (buttonImage == null) continue;

                string optionName = buttonCanvas.Name ?? buttonImage.Name ?? "unnamed_button";
                optionName = optionName.Replace(" ", "_").ToLower();

                // Create the path for the DDS file
                string directoryPath = Path.Combine(ModManager.Directory, "gfx", "superevent", "button");
                string filePath = Path.Combine(directoryPath, $"{CurrentConfig.Id}_{optionName}_bg.dds");

                // Ensure directory exists
                Directory.CreateDirectory(directoryPath);

                // Get the image source
                if (buttonImage.Source is BitmapSource bitmapSource)
                {
                    if(buttonImage.Source != Properties.Resources.null_item_image.ToBitmapSource())
                    {
                        // Convert to DDS (BC3 RGBA format)
                        byte[] ddsBytes = bitmapSource.ConvertToDdsBC3();

                        // Save the file
                        File.WriteAllBytes(filePath, ddsBytes);

                        System.Windows.MessageBox.Show($"Saved button image to: {filePath}");
                    }
                }
            }
        }
        public void CreateCustomAssetsFiles()
        {
            if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id))
            {
                return; 
            }

            string soundDirectory = Path.Combine(ModManager.Directory, "sound");
            string customSoundDirectory = Path.Combine(soundDirectory, "customsound");

            Directory.CreateDirectory(soundDirectory);
            Directory.CreateDirectory(customSoundDirectory);

            UpdateSupereventsAsset(soundDirectory);
            CreateSoundAsset(soundDirectory);
            CreateSoundEffectAsset(soundDirectory);
        }
        private void UpdateSupereventsAsset(string soundDirectory)
        {
            string assetPath = Path.Combine(soundDirectory, "superevents.asset");
            List<string> soundEffects = new List<string>();

            if (File.Exists(assetPath))
            {
                string[] lines = File.ReadAllLines(assetPath);
                bool inSoundEffectsSection = false;

                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("soundeffects = {"))
                    {
                        inSoundEffectsSection = true;
                        continue;
                    }

                    if (inSoundEffectsSection)
                    {
                        if (line.Trim().StartsWith("}"))
                        {
                            inSoundEffectsSection = false;
                            continue;
                        }

                        string cleanedLine = line.Split('/')[0].Trim();
                        if (!string.IsNullOrEmpty(cleanedLine))
                        {
                            soundEffects.Add(cleanedLine);
                        }
                    }
                }
            }
            string newSoundEffect = $"{CurrentConfig.Id}_soundeffect";
            if (!soundEffects.Contains(newSoundEffect))
            {
                soundEffects.Add(newSoundEffect);
            }

            using (StreamWriter writer = new StreamWriter(assetPath))
            {
                writer.WriteLine("category = {");
                writer.WriteLine("\tname = \"SuperEventsSoundEffects\"");
                writer.WriteLine("\tsoundeffects = {");

                foreach (string effect in soundEffects)
                {
                    writer.WriteLine($"\t\t{effect}");
                }

                writer.WriteLine("\t}");
                writer.WriteLine("\tcompressor = {");
                writer.WriteLine("\t\tenabled = yes");
                writer.WriteLine("\t\tpregain = 3.0");
                writer.WriteLine("\t\tpostgain = 0.0");
                writer.WriteLine("\t\tratio = 10.0");
                writer.WriteLine("\t\tthreshold = -15.0");
                writer.WriteLine("\t\tattacktime = 0.030");
                writer.WriteLine("\t\treleasetime = 1.2");
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }
        private void CreateSoundAsset(string soundDirectory)
        {
            string assetPath = Path.Combine(soundDirectory, $"{CurrentConfig.Id}_sound.asset");

            using (StreamWriter writer = new StreamWriter(assetPath))
            {
                writer.WriteLine("sound = { ");
                writer.WriteLine($"\tname = \"{CurrentConfig.Id}_sound\"");
                writer.WriteLine($"\tfile = \"customsound/{CurrentConfig.Id}_sound.wav\"");
                writer.WriteLine("\talways_load = no");
                writer.WriteLine("}");
            }
        }
        private void CreateSoundEffectAsset(string soundDirectory)
        {
            string assetPath = Path.Combine(soundDirectory, $"{CurrentConfig.Id}_soundeffect.asset");

            using (StreamWriter writer = new StreamWriter(assetPath))
            {
                writer.WriteLine("soundeffect = { ");
                writer.WriteLine($"\tname = {CurrentConfig.Id}_soundeffect");
                writer.WriteLine("\tloop = no");
                writer.WriteLine("\tsounds = {");
                writer.WriteLine($"\t\tsound = {CurrentConfig.Id}_sound");
                writer.WriteLine("\t}");
                writer.WriteLine("\tis3d = no");
                writer.WriteLine("\tmax_audible = 1");
                writer.WriteLine("\tmax_audible_behaviour = fail");
                writer.WriteLine("\tvolume = 1.5");
                writer.WriteLine("}");
            }
        }
        public void SaveSupereventImages()
        {
            System.Windows.Controls.Image frame = CurrentConfig.EventConstructor.Children.OfType<System.Windows.Controls.Image>().Where(i => i.Name == "SupereventFrame").FirstOrDefault();
            System.Windows.Controls.Image image = CurrentConfig.EventConstructor.Children.OfType<System.Windows.Controls.Image>().Where(i => i.Name == "SupereventImage").FirstOrDefault();
            if (CurrentConfig == null)
                return;

            string basePath = ModManager.Directory;
            if (image != null)
            {
                string imageDir = Path.Combine(basePath, "gfx", "superevent_pictures");
                Directory.CreateDirectory(imageDir);

                int targetWidth = (int)image.Width;
                int targetHeight = (int)image.Height;

                var renderBitmap = new RenderTargetBitmap(
                    targetWidth,
                    targetHeight,
                    96, 96, // DPI
                    PixelFormats.Pbgra32
                );
                image.Measure(new System.Windows.Size(targetWidth, targetHeight));
                image.Arrange(new Rect(new System.Windows.Size(targetWidth, targetHeight)));
                renderBitmap.Render(image);

                BitmapSource scaledSource = renderBitmap;

                using (var originalImage = ImageManager.ConvertToDrawingImage(scaledSource))
                {
                    if (originalImage != null)
                    {
                        using (var resizedImage = new Bitmap(originalImage, new System.Drawing.Size(targetWidth, targetHeight)))
                        {
                            resizedImage.ConvertToImageSharp().SaveAsTGA(Path.Combine(imageDir, $"superevent_image_{CurrentConfig.Id}.tga")
                            );
                        }
                    }
                }
            }


            if (frame != null)
            {
                string frameDir = Path.Combine(basePath, "gfx", "interface", "superevent");
                string frameFilename = $"superevent_frame_{CurrentConfig.Id}";

                using (var drawingImage = ImageManager.ConvertToDrawingImage(frame.Source.ToBitmapSource()))
                {
                    if (drawingImage != null)
                    {
                        drawingImage.SaveAsDDS(
                            frameDir,
                            frameFilename,
                            (int)frame.ActualWidth,
                            (int)frame.ActualHeight
                        );
                    }
                }
            }
        }
        public void HandleGFXFile()
        {
            string guiDirectory = Path.Combine(ModManager.Directory, "interface");
            Directory.CreateDirectory(guiDirectory);
            string filePath = Path.Combine(guiDirectory, $"SUPEREVENT_{CurrentConfig.Id}_window.gfx");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write main sprites
                writer.WriteLine("spriteTypes = {");
                writer.WriteLine("\tspriteType = {");
                writer.WriteLine($"\t\tname = \"GFX_superevent_{CurrentConfig.Id}_image\"");
                writer.WriteLine($"\t\ttextureFile = \"gfx\\\\superevent_pictures\\\\superevent_image_{CurrentConfig.Id}.tga\"");
                writer.WriteLine("\t}");
                writer.WriteLine("\tspriteType = {");
                writer.WriteLine($"\t\tname = \"GFX_superevent_{CurrentConfig.Id}_bakground\"");
                writer.WriteLine($"\t\ttextureFile = \"gfx\\\\interface\\\\superevent\\\\superevent_frame_{CurrentConfig.Id}.dds\"");
                writer.WriteLine("\t}");

                // Add button sprites with optionChar (A, B, C...)
                var mainCanvas = CurrentConfig.EventConstructor;
                if (mainCanvas != null)
                {
                    int optionIndex = 0;
                    foreach (var child in mainCanvas.Children)
                    {
                        if (child is Canvas optionCanvas && optionCanvas.Name?.StartsWith("OptionButton") == true)
                        {
                            if (optionCanvas.FindWrappedImage().Source.ToBitmapSource() != Properties.Resources.null_item_image.ToBitmapSource())
                            {
                                char optionChar = (char)('A' + optionIndex);
                                string optionName = optionCanvas.Name ?? "unnamed_button";
                                optionName = optionName.Replace(" ", "_").ToLower();
                                writer.WriteLine("\tspriteType = {");
                                writer.WriteLine($"\t\tname = \"GFX_{CurrentConfig.Id}_option_{optionChar}_bg\"");
                                writer.WriteLine($"\t\ttextureFile = \"gfx\\\\superevent\\\\button\\\\{CurrentConfig.Id}_{optionName}_bg.dds\"");
                                writer.WriteLine("\t\tnoOfFrames = 1");
                                writer.WriteLine("\t\teffectFile = \"gfx/FX/buttonstate.lua\"");
                                writer.WriteLine("\t}");

                                optionIndex++;
                            }
                        }
                    }
                }

                writer.WriteLine("}");
            }
        }
        public void HandleGUIFile()
        {
            if (CurrentConfig == null || CurrentConfig.EventConstructor == null || string.IsNullOrEmpty(CurrentConfig.Id))
            {
                return;
            }

            var mainCanvas = CurrentConfig.EventConstructor;
            double canvasWidth = mainCanvas.ActualWidth;
            double canvasHeight = mainCanvas.ActualHeight;
            double centerX = canvasWidth / 2;
            double centerY = canvasHeight / 2;

            string guiDirectory = Path.Combine(ModManager.Directory, "interface");
            Directory.CreateDirectory(guiDirectory);
            string filePath = Path.Combine(guiDirectory, $"SUPEREVENT_{CurrentConfig.Id}_window.gui");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("guiTypes = {");
                writer.WriteLine("\tcontainerWindowType = {");
                writer.WriteLine($"\t\tname = \"SUPEREVENT_{CurrentConfig.Id}_window\"");
                writer.WriteLine($"\t\tsize = {{ width = {canvasWidth} height = {canvasHeight} }}");
                writer.WriteLine("\t\tposition = { x=0 y=0 }");
                writer.WriteLine("\t\tOrientation = center");
                writer.WriteLine("\t\tOrigo = center");
                writer.WriteLine("\t\tclipping = no");
                writer.WriteLine($"\t\tshow_sound = {CurrentConfig.Id}_soundeffect");
                writer.WriteLine();

                // Находим фоновое изображение (рамку)
                var frame = mainCanvas.FindVisualChild<System.Windows.Controls.Image>("SupereventFrame");
                if (frame == null) return;

                // Фоновая рамка - смещаем так, чтобы центр был в (0,0)
                double frameX = -frame.ActualWidth / 2;
                double frameY = -frame.ActualHeight / 2;

                writer.WriteLine("\t\ticonType = {");
                writer.WriteLine("\t\t\tname = \"background\"");
                writer.WriteLine($"\t\t\tspriteType = \"GFX_superevent_{CurrentConfig.Id}_bakground\"");
                writer.WriteLine($"\t\t\tposition = {{ x = {frameX:0} y = {frameY:0} }}");
                writer.WriteLine("\t\t\tOrientation = center");
                writer.WriteLine("\t\t\talwaystransparent = yes");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                // Основное изображение - учитываем смещение на канвасе + центрирование
                var image = mainCanvas.FindVisualChild<System.Windows.Controls.Image>("SupereventImage");
                if (image != null)
                {
                    // Смещение на канвасе
                    double imgX = Canvas.GetLeft(image);
                    double imgY = Canvas.GetTop(image);


                    // Преобразование относительно центра окна
                    imgX -= centerX;
                    imgY -= centerY;

                    writer.WriteLine("\t\ticonType = {");
                    writer.WriteLine("\t\t\tname = \"image\"");
                    writer.WriteLine($"\t\t\tspriteType = \"GFX_superevent_{CurrentConfig.Id}_image\"");
                    writer.WriteLine($"\t\t\tposition = {{ x = {imgX:0} y = {imgY:0} }}");
                    writer.WriteLine("\t\t\tOrientation = center");
                    writer.WriteLine("\t\t\talwaystransparent = yes");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine();
                }

                // Заголовок - учитываем смещение на канвасе + центрирование
                var header = mainCanvas.FindVisualChild<System.Windows.Controls.RichTextBox>( "HeaderTextLocalizedField");
                if (header != null)
                {
                    // Смещение на канвасе
                    double headerX = Canvas.GetLeft(header);
                    double headerY = Canvas.GetTop(header);


                    // Преобразование относительно центра окна
                    headerX -= centerX;
                    headerY -= centerY;

                    writer.WriteLine("\t\tinstantTextBoxType = {");
                    writer.WriteLine("\t\t\tname = \"Title\"");
                    writer.WriteLine($"\t\t\tposition = {{ x = {headerX:0} y = {headerY:0} }}");
                    writer.WriteLine($"\t\t\tfont = \"{(header.FontFamily+"_"+ header.FontSize).ToLowerInvariant()}\"");
                    writer.WriteLine("\t\t\tborderSize = {x = 0 y = 0}");
                    writer.WriteLine($"\t\t\ttext = \"SUPEREVENT_{CurrentConfig.Id}_TITLE\"");
                    writer.WriteLine($"\t\t\tmaxWidth = {header.ActualWidth:0}");
                    writer.WriteLine($"\t\t\tmaxHeight = {header.ActualHeight:0}");
                    writer.WriteLine("\t\t\tOrientation = center");
                    writer.WriteLine("\t\t\tformat = centre");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine();
                }

                // Описание - учитываем смещение на канвасе + ориентация lower_left
                var desc = mainCanvas.FindVisualChild<System.Windows.Controls.RichTextBox>("DescTextLocalizedField");
                if (desc != null)
                {
                    // Смещение на канвасе
                    double descX = Canvas.GetLeft(desc);
                    double descY = Canvas.GetTop(desc); // нижний край

                    // Преобразование относительно центра окна
                    descX -= centerX;
                    descY -= centerY;

                    writer.WriteLine("\t\tinstantTextBoxType = {");
                    writer.WriteLine("\t\t\tname = \"Desc\"");
                    writer.WriteLine($"\t\t\tposition = {{ x = {descX:0} y = {descY:0} }}");
                    writer.WriteLine($"\t\t\tfont = \"{(desc.FontFamily+"_"+desc.FontSize).ToLowerInvariant()}\"");
                    writer.WriteLine($"\t\t\ttext = \"SUPEREVENT_{CurrentConfig.Id}_DESC\"");
                    writer.WriteLine($"\t\t\tmaxWidth = {desc.ActualWidth:0}");
                    writer.WriteLine($"\t\t\tmaxHeight = {desc.ActualHeight:0}");
                    writer.WriteLine("\t\t\tfixedsize = yes");
                    writer.WriteLine("\t\t\tOrientation = center");
                    writer.WriteLine("\t\t\tformat = centre");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine();
                }

                // Кнопки - учитываем смещение на канвасе + центрирование
                int optionIndex = 0;
                foreach (var child in mainCanvas.Children)
                {
                    if (child is Canvas optionCanvas && optionCanvas.Name?.StartsWith("OptionButton") == true)
                    {
                        char optionChar = (char)('A' + optionIndex);

                        // Смещение на канвасе
                        double btnX = Canvas.GetLeft(optionCanvas);
                        double btnY = Canvas.GetTop(optionCanvas);

                        // Преобразование относительно центра окна
                        btnX -= centerX;
                        btnY -= centerY;

                        writer.WriteLine("\t\tbuttonType = {");
                        writer.WriteLine($"\t\t\tname = \"Option{optionChar}\"");
                        writer.WriteLine($"\t\t\ttext = \"SUPEREVENT_{CurrentConfig.Id}_OPTION_{optionChar}\"");

                        if (optionIndex == 0)
                        {
                            writer.WriteLine("\t\t\tshortcut = \"ESCAPE\"");
                        }

                        writer.WriteLine($"\t\t\tposition = {{ x = {btnX:0} y = {btnY:0} }}");
                        if (optionCanvas.FindWrappedImage().Source.ToBitmapSource() != Properties.Resources.null_item_image.ToBitmapSource())
                        {
                            writer.WriteLine($"\t\t\tquadTextureSprite =\"GFX_{CurrentConfig.Id}_option_{optionChar}_bg\"");
                        }
                        else
                        {
                            writer.WriteLine($"\t\t\tquadTextureSprite =\"GFX_button_221x34\"");
                        }
                        writer.WriteLine($"\t\t\tbuttonFont = \"{(optionCanvas.FindWrappedTextBox().FontFamily + "_" + optionCanvas.FindWrappedTextBox().FontSize).ToLowerInvariant()}\"");
                        writer.WriteLine("\t\t\tOrientation = center");
                        writer.WriteLine("\t\t}");
                        writer.WriteLine();

                        optionIndex++;
                    }
                }

                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }
        public void HandleLocalizationFiles()
        {
            if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id))
            {
                return;
            }

            try
            {
                HandleLocalizationFile("russian", "l_russian:",
                    CurrentConfig.Header, CurrentConfig.Description);

                HandleLocalizationFile("english", "l_english:",
                    "", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании файлов локализации: {ex.Message}");
            }
        }
        public void HandleFontFiles()
        {
            string fontsDirectory = Path.Combine(ModManager.Directory, "gfx", "fonts");
            Directory.CreateDirectory(fontsDirectory);

            var uniqueFonts = new HashSet<FontSignature>();
            FontSignature.CollectUniqueFonts(CurrentConfig.EventConstructor, uniqueFonts);

            try
            {
                foreach (var fontSignature in uniqueFonts)
                {
                    string fontFileName = fontSignature.GenerateFontName();
                    string fntPath = Path.Combine(fontsDirectory, $"{fontFileName}.fnt");
                    string ddsPath = Path.Combine(fontsDirectory, $"{fontFileName}.dds");
                    string tgaPath = Path.Combine(fontsDirectory, $"{fontFileName}.tga");

                    if (File.Exists(fntPath) && (File.Exists(ddsPath) || File.Exists(tgaPath))) continue;

                    fontSignature.HandleFontFolderWithChecks(fontsDirectory);
                }
            }
            catch(OperationCanceledException) 
            {
                if (Debugger.Instance != null)
                {
                    Debugger.Instance.LogMessage("[WPF EXEPTION]: Операция отменена пользователем или из-за ошибок покрытия шрифта.");
                }
                else
                {
                    Debug.WriteLine("[WPF EXEPTION]: Операция отменена пользователем или из-за ошибок покрытия шрифта.");
                }
                
            }
            catch (Exception ex)
            {
                if (Debugger.Instance != null)
                {
                    MessageBox.Show($"Не удалось завершить операцию:\n{ex.Message}",
                                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debugger.Instance.LogMessage($"[WPF EXEPTION]: Не удалось завершить операцию:{ex.Message}");
                }
                else
                {
                    MessageBox.Show($"Не удалось завершить операцию:\n{ex.Message}",
                               "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void HandleLocalizationFile(string language, string header, string titleValue, string descValue)
        {
            var mainCanvas = CurrentConfig.EventConstructor;
            string locDirectory = Path.Combine(ModManager.Directory, "localisation", language);
            Directory.CreateDirectory(locDirectory);

            string filePath = Path.Combine(locDirectory, $"superevents_l_{language}.yml");
            List<string> lines = new List<string>();

            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            }
            if (lines.Count == 0 || !lines[0].StartsWith(header))
            {
                lines.Insert(0, header);
            }
            string titleKey = $" SUPEREVENT_{CurrentConfig.Id}_TITLE";
            string descKey = $" SUPEREVENT_{CurrentConfig.Id}_DESC";
            int optionIndex = 0;
            foreach (var child in mainCanvas.Children)
            {
                if (child is Canvas optionCanvas && optionCanvas.Name?.StartsWith("OptionButton") == true)
                {
                    char optionChar = (char)('A' + optionIndex);
                    string optionText = optionCanvas.FindWrappedTextBox().GetTextFromRichTextBox();
                    lines.Add($" SUPEREVENT_{CurrentConfig.Id}_OPTION_{optionChar}: \"§M{EscapeYamlString(optionText)}\"");
                    optionIndex++;
                }
            }
            lines = lines.Where(line =>
                !line.StartsWith(titleKey + ":") &&
                !line.StartsWith(descKey + ":")).ToList();

            lines.Add($" {titleKey}: \"§M{EscapeYamlString(titleValue)}\"");
            lines.Add($" {descKey}: \"§M{EscapeYamlString(descValue)}\"");
           
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        private string EscapeYamlString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Удалить \r и \n, заменить на пробел, затем экранировать кавычки
            input = input.Replace("\r", "").Replace("\n", " ").Trim();
            return input.Replace("\"", "\\\"");
        }

        public void HandleScriptedGuiFile()
        {
            if (CurrentConfig == null || string.IsNullOrEmpty(CurrentConfig.Id) || CurrentConfig.EventConstructor == null)
            {
                return;
            }
            string scriptedGuiDirectory = Path.Combine(ModManager.Directory, "common", "scripted_guis");
            Directory.CreateDirectory(scriptedGuiDirectory);

            string filePath = Path.Combine(scriptedGuiDirectory, $"SUPEREVENT_{CurrentConfig.Id}_scripted_gui.txt");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("scripted_gui = {");
                writer.WriteLine();
                writer.WriteLine($"\tSUPEREVENT_{CurrentConfig.Id}_window = {{ ");
                writer.WriteLine("\t\tcontext_type = player_context");
                writer.WriteLine($"\t\twindow_name = \"SUPEREVENT_{CurrentConfig.Id}_window\"");
                writer.WriteLine();
                writer.WriteLine("\t\tvisible = {");
                writer.WriteLine($"\t\t\thas_country_flag = superevent_{CurrentConfig.Id}_flag");
                writer.WriteLine("\t\t}");
                writer.WriteLine();
                writer.WriteLine("\t\teffects = {");

                int optionIndex = 0;
                foreach (var child in CurrentConfig.EventConstructor.Children)
                {
                    if (child is Canvas optionCanvas && optionCanvas.Name?.StartsWith("OptionButton") == true)
                    {
                        char optionChar = (char)('A' + optionIndex);

                        writer.WriteLine($"\t\t\tOption{optionChar}_click = {{");
                        writer.WriteLine($"\t\t\t\tclr_country_flag = superevent_{CurrentConfig.Id}_flag");
                        writer.WriteLine("\t\t\t}");

                        optionIndex++;
                    }
                }

                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        public void HandleFontDefineFiles()
        {
            string interfaceDirectory = Path.Combine(ModManager.Directory, "interface");
            Directory.CreateDirectory(interfaceDirectory);
            string filePath = Path.Combine(interfaceDirectory, "font_definitions.gfx");

            // Собираем все уникальные шрифты из интерфейса
            var uniqueFonts = new HashSet<FontSignature>();
            FontSignature.CollectUniqueFonts(CurrentConfig.EventConstructor, uniqueFonts);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("bitmapfonts = {");

                foreach (var fontSignature in uniqueFonts)
                {
                    // Преобразуем цвет в формат RGB (0.0-1.0)
                    var color = fontSignature.Color;
                    int r = color.R;
                    int g = color.G;
                    int b = color.B;

                    // Формируем запись для шрифта
                    writer.WriteLine("\tbitmapfont = {");
                    writer.WriteLine($"\t\tname = \"{fontSignature.Name}\"");
                    writer.WriteLine("\t\tfontfiles = {");
                    writer.WriteLine($"\t\t\t\"gfx/fonts/{fontSignature.Name}\"");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine("\t\tcolor = 0xffffffff");
                    writer.WriteLine("\t\ttextcolors = {");
                    writer.WriteLine($"\t\t\tM = {{ {r:0} {g:0} {b:0} }} // Основной цвет");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine("\t}");
                    writer.WriteLine();
                }

                writer.WriteLine("}");
            }
        }
    }
}
