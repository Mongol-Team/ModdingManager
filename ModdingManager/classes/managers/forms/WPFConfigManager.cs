
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO.Compression;
using ModdingManager.configs;
using Microsoft.VisualBasic;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using SixLabors.ImageSharp;
using ModdingManager.classes.extentions;

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

        private static async Task<BitmapSource> LoadImageFromFile(string filePath)
        {
            return await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            });
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
        static BitmapSource EnsureRenderable(BitmapSource source)
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

        


        private static async Task SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
        {
            await Task.Run(() =>
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(fileStream);
                }
            });
        }

        private static async Task SaveImageToPng(ImageSource image, string filePath)
        {
            if (image is BitmapSource bitmapSource)
            {
                await Task.Run(() =>
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }
                });
            }
        }

        private static async Task<List<string>> GetAvailableTechTreeConfigsAsync()
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(TechTreesPath))
                {
                    Directory.CreateDirectory(TechTreesPath);
                    return new List<string>();
                }

                return Directory.GetFiles(TechTreesPath, "*.tech")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
            });
        }

        private static async Task<TechTreeConfig> LoadTechTreeFromArchive(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                var jsonEntry = archive.GetEntry("config.json");
                using (var reader = new StreamReader(jsonEntry.Open()))
                {
                    string json = await reader.ReadToEndAsync();
                    var config = JsonSerializer.Deserialize<TechTreeConfig>(json);

                    foreach (var item in config.Items)
                    {
                        var imageEntry = archive.GetEntry($"images/{item.Id}.png");
                        if (imageEntry != null)
                        {
                            using (var stream = imageEntry.Open())
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.StreamSource = stream;
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                item.Image = bitmap;
                            }
                        }
                        else
                        {
                            // Используем стандартный фон
                            string defaultImagePath = item.IsBig
                                ? @"ModdingManager\ModdingManager\data\gfx\interface\technology_available_item_bg.png"
                                : @"ModdingManager\ModdingManager\data\gfx\interface\tech_industry_available_item_bg.png";

                            if (File.Exists(defaultImagePath))
                            {
                                var uri = new Uri(defaultImagePath, UriKind.RelativeOrAbsolute);
                                item.Image = new BitmapImage(uri);
                            }
                        }
                    }

                    return config;
                }
            }
        }

        private static async Task SaveTechTreeToArchive(TechTreeConfig config, string filePath)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var jsonEntry = archive.CreateEntry("config.json");
                    using (var writer = new StreamWriter(jsonEntry.Open()))
                    {
                        string json = JsonSerializer.Serialize(config, JsonOptions);
                        await writer.WriteAsync(json);
                    }

                    foreach (var item in config.Items.Where(i => i.Image != null))
                    {
                        var imageEntry = archive.CreateEntry($"images/{item.Id}.png");
                        using (var stream = imageEntry.Open())
                        {
                            if (item.Image is BitmapSource bitmapSource)
                            {
                                var encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                                encoder.Save(stream);
                            }
                        }
                    }
                }

                await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
            }
        }

        public static async Task SaveConfigWrapper(TechTreeCreator window)
        {
            string fileName = await ShowWpfInputDialog(
                window,
                "Save Configuration",
                "Enter file name (without .tech extension):");

            if (string.IsNullOrWhiteSpace(fileName))
            {
                System.Windows.MessageBox.Show(
                    window,
                    "Save operation canceled!",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            await SaveConfigAsync(window, fileName);
        }

        private static async Task<string> ShowWpfInputDialog(Window owner, string title, string prompt)
        {
            var tcs = new TaskCompletionSource<string>();

            await owner.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new Window
                {
                    Title = title,
                    Width = 350,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    SizeToContent = SizeToContent.Manual
                };

                var textBox = new System.Windows.Controls.TextBox
                {
                    Margin = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Top
                };

                var button = new System.Windows.Controls.Button
                {
                    Content = "OK",
                    Width = 80,
                    Margin = new Thickness(10),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    IsDefault = true
                };

                button.Click += (s, e) =>
                {
                    tcs.SetResult(textBox.Text);
                    dialog.Close();
                };

                var stackPanel = new System.Windows.Controls.StackPanel();
                stackPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = prompt,
                    Margin = new Thickness(10, 10, 10, 5)
                });
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(button);

                dialog.Content = stackPanel;
                dialog.Owner = owner;
                dialog.ShowDialog();
            });

            return await tcs.Task;
        }
        private static async Task<string> ShowWpfSelectionDialog(Window owner, List<string> items, string title)
        {
            var tcs = new TaskCompletionSource<string>();

            await owner.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new Window
                {
                    Title = title,
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var listBox = new System.Windows.Controls.ListBox
                {
                    ItemsSource = items,
                    FontSize = 14,
                    SelectionMode = System.Windows.Controls.SelectionMode.Single
                };

                var button = new System.Windows.Controls.Button
                {
                    Content = "Load",
                    Height = 40,
                    IsDefault = true
                };

                button.Click += (s, e) =>
                {
                    tcs.SetResult(listBox.SelectedItem?.ToString());
                    dialog.Close();
                };

                var stackPanel = new System.Windows.Controls.StackPanel();
                stackPanel.Children.Add(listBox);
                stackPanel.Children.Add(button);

                dialog.Content = stackPanel;
                dialog.Owner = owner;
                dialog.ShowDialog();
            });

            return await tcs.Task;
        }
    }
}