using ModdingManager.classes.extentions;
using ModdingManager.configs;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModdingManager.managers.forms
{
    public static class WpfConfigManager
    {
        private static readonly string ConfigsPath = Path.Combine("..", "..", "..", "data", "configs");
        private static readonly string TechTreesPath = Path.Combine(ConfigsPath, "techtrees");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task LoadConfigAsync(TechTreeCreator window)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Tech Tree Files (*.tech)|*.tech",
                InitialDirectory = TechTreesPath
            };

            openDialog.ShowDialog();

            try
            {
                window.IsEnabled = false;
                window.Cursor = System.Windows.Input.Cursors.Wait;

                var (config, imageData) = await Task.Run(() =>
                {
                    using (var archive = ZipFile.OpenRead(openDialog.FileName))
                    {
                        var jsonEntry = archive.GetEntry("config.json");
                        var config = JsonSerializer.Deserialize<TechTreeConfig>(
                            new StreamReader(jsonEntry.Open()).ReadToEnd());

                        var images = new Dictionary<string, byte[]>();
                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FullName.StartsWith("images/") && entry.Name.EndsWith(".png"))
                            {
                                using (var ms = new MemoryStream())
                                {
                                    entry.Open().CopyTo(ms);
                                    images.Add(Path.GetFileName(entry.Name), ms.ToArray());
                                }
                            }
                        }

                        return (config, images);
                    }
                });

                var rootImages = await Task.Run(() =>
                {
                    using (var archive = ZipFile.OpenRead(openDialog.FileName))
                    {
                        var images = new Dictionary<string, byte[]>();

                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.FullName.StartsWith("images/") && entry.Name.EndsWith(".png"))
                            {
                                using (var ms = new MemoryStream())
                                {
                                    entry.Open().CopyTo(ms);
                                    images.Add(entry.Name, ms.ToArray());
                                }
                            }
                        }

                        return images;
                    }
                });

                await window.Dispatcher.InvokeAsync(() =>
                {
                    window.CurrentTechTree = config;

                    if (rootImages.TryGetValue($"{config.Name}_first.png", out var firstImageBytes))
                    {
                        window.TabFolderFirstImage.Source = LoadImageFromBytes(firstImageBytes);
                    }

                    if (rootImages.TryGetValue($"{config.Name}_second.png", out var secondImageBytes))
                    {
                        window.TabFolderSecondImage.Source = LoadImageFromBytes(secondImageBytes);
                    }

                    if (rootImages.TryGetValue($"background.png", out var bgimageBytes))
                    {
                        window.TechBGImage.ImageSource = LoadImageFromBytes(bgimageBytes);
                    }

                    foreach (var item in window.CurrentTechTree.Items)
                    {
                        if (imageData.TryGetValue($"{item.Id}.png", out var bytes))
                        {
                            item.Image = LoadImageFromBytes(bytes);
                        }
                        else
                        {
                            item.Image = LoadDefaultBackground(item.IsBig);
                        }
                    }

                    window.RefreshTechTreeView();
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                window.IsEnabled = true;
                window.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private static BitmapSource LoadImageFromBytes(byte[] imageData)
        {
            var bitmap = new BitmapImage();
            using (var ms = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            bitmap.Freeze(); // Теперь это безопасно
            return bitmap;
        }

        private static ImageSource LoadDefaultBackground(bool isBig)
        {
            string path = isBig
                ? @"ModdingManager\data\gfx\interface\technology_available_item_bg.png"
                : @"ModdingManager\data\gfx\interface\tech_industry_available_item_bg.png";

            if (File.Exists(path))
            {
                var bitmap = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                bitmap.Freeze();
                return bitmap;
            }
            return null;
        }


        private static BitmapSource FreezeClone(ImageSource source)
        {
            if (source == null)
                return null;

            var clone = (BitmapSource)source.Clone();
            clone.Freeze();
            return clone;
        }


        public static async Task SaveConfigAsync(TechTreeCreator window, string configName)
        {
            string archivePath = Path.Combine(TechTreesPath, $"{configName}.tech");
            Directory.CreateDirectory(TechTreesPath);

            var saveData = await window.Dispatcher.InvokeAsync(() =>
            {
                return new
                {
                    Config = JsonSerializer.Serialize(window.CurrentTechTree, JsonOptions),
                    Images = window.CurrentTechTree.Items
                        .Where(item => item.Image != null)
                        .ToDictionary(
                            item => $"{item.Id}.png",
                            item =>
                            {
                                var imgCopy = item.Image.Clone();
                                imgCopy.Freeze();
                                return (BitmapSource)imgCopy;
                            }
                        ),
                    SecondTabImage = FreezeClone(window.TabFolderSecondImage.Source),
                    FirstTabImage = FreezeClone(window.TabFolderFirstImage.Source),
                    BgImage = FreezeClone(window.TechBGImage.ImageSource),
                };
            });


            await Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        var jsonEntry = archive.CreateEntry("config.json");
                        using (var writer = new StreamWriter(jsonEntry.Open()))
                        {
                            writer.Write(saveData.Config);
                        }

                        foreach (var img in saveData.Images)
                        {
                            var imageEntry = archive.CreateEntry($"images/{img.Key}");
                            using (var stream = imageEntry.Open())
                            {
                                EnsureRenderable(img.Value).SaveToStream(stream);
                            }
                        }

                        var firstImageEntry = archive.CreateEntry($"{window.CurrentTechTree.Name}_first.png");
                        var first = saveData.FirstTabImage;
                        using (var stream = firstImageEntry.Open())
                        {
                            EnsureRenderable(first).SaveToStream(stream);
                        }
                        var secondImageEntry = archive.CreateEntry($"{window.CurrentTechTree.Name}_second.png");
                        var second = saveData.SecondTabImage;
                        using (var stream = secondImageEntry.Open())
                        {
                            EnsureRenderable(second).SaveToStream(stream);
                        }
                        var backgroundImageEntry = archive.CreateEntry($"background.png");
                        var bg = saveData.BgImage;
                        using (var stream = backgroundImageEntry.Open())
                        {
                            EnsureRenderable(bg).SaveToStream(stream);
                        }
                    }

                    File.WriteAllBytes(archivePath, memoryStream.ToArray());
                }
            });

            await window.Dispatcher.InvokeAsync(() =>
                System.Windows.MessageBox.Show("Сохранение завершено!", "Успех"));
        }
        private static BitmapSource EnsureRenderable(BitmapSource source)
        {
            if (source is TransformedBitmap || source is CroppedBitmap || source is FormatConvertedBitmap)
            {
                var rtb = new RenderTargetBitmap(
                    source.PixelWidth, source.PixelHeight,
                    source.DpiX, source.DpiY, PixelFormats.Pbgra32);

                var visual = new DrawingVisual();
                using (var ctx = visual.RenderOpen())
                {
                    ctx.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
                }

                rtb.Render(visual);
                rtb.Freeze();
                return rtb;
            }

            return source;
        }



    }
}