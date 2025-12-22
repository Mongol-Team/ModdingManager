//OTM
//namespace ModdingManager.classes.managers.@base
//{
//    using global::Application.Extentions
//    using global::ModdingManager.classes.utils;
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.IO.Compression;
//    using System.Reflection;
//    using System.Text.Json;
//    using System.Threading.Tasks;
//    using System.Windows;
//    using System.Windows.Media;
//    using System.Windows.Media.Imaging;

//    public class ConfigManager
//    {
//        private static readonly string BaseConfigsPath = GetApplicationConfigPath();
//        private static readonly string CountryConfigsPath = Path.Combine(BaseConfigsPath, "countries");



//        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
//        {
//            WriteIndented = true,
//            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
//        };

//        public static async Task SaveConfigAsync<TView>(TView view) where TView : Window
//        {
//            if (view is ICountryView countryView)
//            {
//                await SaveCountryConfigAsync(view);
//            }
//        }

//        public static async Task LoadConfigAsync<TView>(TView view) where TView : Window
//        {
//            if (view is ICountryView countryView)
//            {
//                await LoadCountryConfigAsync(view);
//            }
//        }
//        #region Events
//        public static void OnSaveConfigClickedEvent(object sender, RoutedEventArgs e)
//        {
//            if (sender is Window window)
//            {
//                HandleSaveConfig(window);
//            }
//        }

//        public static void OnLoadConfigClickedEvent(object sender, RoutedEventArgs e)
//        {
//            if (sender is Window window)
//            {
//                HandleLoadConfig(window);
//            }
//        }

//        private static async void HandleSaveConfig(Window window)
//        {
//            try
//            {
//                window.IsEnabled = false;
//                window.Cursor = System.Windows.Input.Cursors.Wait;

//                if (window is ICountryView countryView)
//                {
//                    await SaveCountryConfigAsync(window);
//                }
//                else
//                {
//                    MessageBox.Show("Тип конфигурации не поддерживается", "Ошибка",
//                        MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
//                    MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//            finally
//            {
//                window.IsEnabled = true;
//                window.Cursor = System.Windows.Input.Cursors.Arrow;
//            }
//        }

//        private static async void HandleLoadConfig(Window window)
//        {
//            try
//            {
//                window.IsEnabled = false;
//                window.Cursor = System.Windows.Input.Cursors.Wait;

//                if (window is ICountryView countryView)
//                {
//                    await LoadCountryConfigAsync(window);
//                }
//                else
//                {
//                    MessageBox.Show("Тип конфигурации не поддерживается", "Ошибка",
//                        MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
//                    MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//            finally
//            {
//                window.IsEnabled = true;
//                window.Cursor = System.Windows.Input.Cursors.Arrow;
//            }
//        }
//        #endregion
//        #region Country View Implementation
//        private static async Task SaveCountryConfigAsync(Window window)
//        {
//            var saveDialog = new Microsoft.Win32.SaveFileDialog
//            {
//                Filter = "Country Config Files (*.country)|*.country",
//                InitialDirectory = CountryConfigsPath
//            };

//            if (saveDialog.ShowDialog() != true) return;
//            var view = window as ICountryView;
//            try
//            {
//                var config = new CountryConfig
//                {
//                    Tag = view.Tag,
//                    Name = view.Name,
//                    Capital = view.Capital,
//                    GraphicalCulture = view.GraphicalCulture,
//                    Color = view.Color,
//                    Technologies = view.Technologies,
//                    Convoys = view.Convoys,
//                    OOB = view.OOB,
//                    CountryFileName = view.CountryFileName,
//                    Stab = view.Stab,
//                    WarSup = view.WarSup,
//                    ResearchSlots = view.ResearchSlots,
//                    RulingParty = view.RulingParty,
//                    LastElection = view.LastElection,
//                    ElectionFrequency = view.ElectionFrequency,
//                    ElectionsAllowed = view.ElectionsAllowed,
//                    PartyPopularities = view.PartyPopularities,
//                    Ideas = view.Ideas,
//                    Characters = view.Characters,
//                    StateCores = view.States
//                };
//                config.StateCores = view.States;
//                await SaveCountryConfigInternalAsync(config, window, saveDialog.FileName);
//                view.ShowMessage("Конфигурация страны успешно сохранена!");
//            }
//            catch (Exception ex)
//            {
//                view.ShowError($"Ошибка сохранения: {ex.Message}");
//            }
//        }

//        private static async Task LoadCountryConfigAsync(Window window)
//        {
//            if (window == null) throw new ArgumentNullException(nameof(window));
//            if (!window.Dispatcher.CheckAccess())
//                throw new InvalidOperationException("Метод должен вызываться из UI-потока");

//            // 1. Подготовка данных для диалога
//            var dialog = new Microsoft.Win32.OpenFileDialog
//            {
//                Filter = "Country Config Files (*.country)|*.country",
//                InitialDirectory = CountryConfigsPath,
//                CheckFileExists = true,
//                CheckPathExists = true
//            };

//            // 2. Синхронное открытие диалога
//            bool? dialogResult;
//            try
//            {
//                dialogResult = dialog.ShowDialog(window);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(window,
//                    $"Не удалось открыть диалог выбора файла:\n{ex.Message}",
//                    "Ошибка",
//                    MessageBoxButton.OK,
//                    MessageBoxImage.Error);
//                return;
//            }

//            if (dialogResult != true) return;
//            ICountryView view = window as ICountryView;

//            try
//            {
//                // Блокируем UI на время загрузки
//                await window.Dispatcher.InvokeAsync(() =>
//                {
//                    if (view is UIElement element)
//                        element.IsEnabled = false;
//                });

//                var config = await LoadCountryConfigInternalAsync(dialog.FileName, window);

//                // Обновляем View в UI-потоке
//                await window.Dispatcher.InvokeAsync(() =>
//                {
//                    view.Tag = config.Tag;
//                    view.Name = config.Name;
//                    view.Capital = config.Capital;
//                    view.GraphicalCulture = config.GraphicalCulture;
//                    view.Color = config.Color;
//                    view.Technologies = config.Technologies;
//                    view.Convoys = config.Convoys;
//                    view.OOB = config.OOB;
//                    view.CountryFileName = config.CountryFileName;
//                    view.Stab = config.Stab;
//                    view.WarSup = config.WarSup;
//                    view.ResearchSlots = config.ResearchSlots;
//                    view.RulingParty = config.RulingParty;
//                    view.LastElection = config.LastElection;
//                    view.ElectionFrequency = config.ElectionFrequency;
//                    view.ElectionsAllowed = config.ElectionsAllowed;
//                    view.PartyPopularities = config.PartyPopularities;
//                    view.Ideas = config.Ideas;
//                    view.Characters = config.Characters;
//                    view.States = config.StateCores;
//                    view.CountryFlags = config.CountryFlags;

//                });

//                view.ShowMessage("Конфигурация страны успешно загружена!");
//            }
//            catch (Exception ex)
//            {
//                view.ShowError($"Ошибка загрузки: {ex.Message}");
//            }
//            finally
//            {
//                // Разблокируем UI
//                await window.Dispatcher.InvokeAsync(() =>
//                {
//                    if (view is UIElement element)
//                        element.IsEnabled = true;
//                });
//            }
//        }

//        private static async Task SaveCountryConfigInternalAsync(
//            CountryConfig config,
//            Window view,
//            string filePath)
//        {
//            var flags = (view as ICountryView).CountryFlags;
//            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

//            using (var memoryStream = new MemoryStream())
//            {
//                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
//                {
//                    // Сохранение конфигурации
//                    var configEntry = archive.CreateEntry("config.json");
//                    using (var writer = new StreamWriter(configEntry.Open()))
//                    {
//                        await writer.WriteAsync(JsonSerializer.Serialize(config, JsonOptions));
//                    }

//                    // Сохранение флагов
//                    foreach (var flag in flags)
//                    {
//                        var flagEntry = archive.CreateEntry($"flags/{flag.Key}.png");
//                        using (var stream = flagEntry.Open())
//                        {
//                            await SaveImageSourceToStream(flag.Values, stream, view);
//                        }
//                    }
//                }

//                await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
//            }
//        }

//        private static async Task<CountryConfig>
//            LoadCountryConfigInternalAsync(string filePath, Window window)
//        {
//            using (var archive = ZipFile.OpenRead(filePath))
//            {
//                // Загрузка конфигурации
//                var configEntry = archive.GetEntry("config.json");
//                CountryConfig config;
//                using (var reader = new StreamReader(configEntry.Open()))
//                {
//                    config = JsonSerializer.Deserialize<CountryConfig>(await reader.ReadToEndAsync());
//                }

//                // Загрузка флагов
//                var flags = new Dictionary<string, ImageSource>();
//                foreach (var entry in archive.Entries)
//                {
//                    if (entry.FullName.StartsWith("flags/") && entry.Name.EndsWith(".png"))
//                    {
//                        var ideologyId = Path.GetFileNameWithoutExtension(entry.Name);
//                        var ideology = Registry.Instance.GetIdeology(ideologyId);

//                        using (var stream = entry.Open())
//                        {
//                            flags[ideology.Id] = await LoadImageSourceFromStream(stream, window);
//                        }
//                    }
//                }
//                config.CountryFlags = flags;
//                return (config);
//            }
//        }
//        #endregion

//        #region Helper Methods
//        private static async Task SaveImageSourceToStream(ImageSource imageSource, Stream stream, Window view)
//        {
//            if (imageSource is BitmapSource bitmapSource)
//            {
//                BitmapSource clonedBitmap = null;

//                // Доступ к UI-объекту через Dispatcher
//                await view.Dispatcher.InvokeAsync(() =>
//                {
//                    // Клонируем изображение, чтобы оно стало доступно вне UI-потока
//                    clonedBitmap = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);
//                });

//                // Сохраняем в PNG
//                clonedBitmap.SaveToStream(stream);
//            }
//        }
//        private static async Task<ImageSource> LoadImageSourceFromStream(Stream stream, Window window)
//        {
//            // Копируем поток в память
//            byte[] imageBytes;
//            using (var ms = new MemoryStream())
//            {
//                await stream.CopyToAsync(ms);
//                imageBytes = ms.ToArray();
//            }

//            // Создаём изображение в UI-потоке
//            return await window.Dispatcher.InvokeAsync(() =>
//            {
//                var bitmap = new BitmapImage();
//                using (var ms = new MemoryStream(imageBytes))
//                {
//                    bitmap.BeginInit();
//                    bitmap.StreamSource = ms;
//                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
//                    bitmap.EndInit();
//                    bitmap.Freeze(); // Теперь безопасно
//                }
//                return (ImageSource)bitmap;
//            });
//        }

//        private static string GetApplicationConfigPath()
//        {
//            // Вариант 1: Рядом с исполняемым файлом
//            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            string path = Path.Combine(exePath, "data", "Models");

//            // Вариант 2: В AppData для текущего пользователя
//            // string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
//            // string path = Path.Combine(appData, "YourAppName", "Models");

//            // Создаем директорию, если её нет
//            Directory.CreateDirectory(path);
//            return path;
//        }
//        #endregion
//    }
//}

