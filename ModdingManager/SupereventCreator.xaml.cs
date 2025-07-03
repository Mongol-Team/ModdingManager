using ModdingManager.classes.controls;
using ModdingManager.classes.extentions;
using ModdingManager.classes.handlers;
using ModdingManager.configs;
using ModdingManager.managers;
using NAudio.Wave;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using RichTextBox = System.Windows.Controls.RichTextBox;
using TextBox = System.Windows.Controls.TextBox;
namespace ModdingManager
{
    public partial class SupereventCreator : Window
    {
        private UIElement selectedElement;
        private System.Windows.Point startPoint;
        private bool isDragging = false;
        private bool isResizing = false;
        private ResizeDirection resizeDirection;
        private System.Windows.Size originalSize;
        private Dictionary<UIElement, Rect> originalChildBounds;

        private Rect elementBounds;
        private Border highlightBorder;
        public SupereventConfig CurrentConfig = new();
        private enum ResizeDirection
        {
            None,
            TopLeft,
            Top,
            TopRight,
            Left,
            Right,
            BottomLeft,
            Bottom,
            BottomRight
        }

        public SupereventCreator()
        {
            InitializeComponent();
            AdjustCanvasSizeToImage();
            InitializeDragAndResize();
            SupereventFrame.IsHitTestVisible = false;
            Debugger.AttachIfDebug(this);
        }
        #region Helper Methods
        private bool IsInEditableRichTextBox(System.Windows.Input.MouseEventArgs e)
        {
            DependencyObject current = e.OriginalSource as DependencyObject;

            while (current != null)
            {
                if (current is System.Windows.Controls.RichTextBox rtb && !rtb.IsReadOnly)
                    return true;

                current = LogicalTreeHelper.GetParent(current);
            }
            return false;
        }


        private void InitializeDragAndResize()
        {
            highlightBorder = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.DodgerBlue,
                BorderThickness = new Thickness(2),
                IsHitTestVisible = false,      // рамка не ловит клики
                Visibility = Visibility.Collapsed
            };

            MainCanvas.Children.Add(highlightBorder);
            System.Windows.Controls.Panel.SetZIndex(highlightBorder, int.MaxValue);

            MainCanvas.PreviewMouseLeftButtonDown += MainCanvas_MouseLeftButtonDown;
            MainCanvas.PreviewMouseRightButtonDown += MainCanvas_MouseRightButtonDown;
            MainCanvas.PreviewMouseMove += MainCanvas_MouseMove;
            MainCanvas.PreviewMouseUp += MainCanvas_MouseUp;
            MainCanvas.MouseLeave += MainCanvas_MouseLeave;
        }

        

        private UIElement GetTopLevelChild(UIElement hit)
        {
            DependencyObject current = hit;
            DependencyObject parent;
            while ((parent = VisualTreeHelper.GetParent(current)) != null)
            {
                if (parent == MainCanvas)
                    return current as UIElement;
                current = parent;
            }
            return hit;
        }

        private bool HandleResize(System.Windows.Point currentPos)
        {
            if (!isResizing || selectedElement == null)
                return false;
            Vector delta = currentPos - startPoint;
            bool isContainer = selectedElement is Canvas w && w != MainCanvas && w.Children.Count > 0;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (isContainer)
                    ResizeContainerCentered((Canvas)selectedElement, currentPos);
                else
                    ResizeElementCentered(selectedElement, currentPos);
            }
            else
            {
                if (isContainer)
                    ResizeContainer((Canvas)selectedElement, delta, resizeDirection);
                else
                    ResizeElement(selectedElement, delta, resizeDirection);
            }
            UpdateHighlightBorder(selectedElement);
            startPoint = currentPos;
            return true;
        }

        private bool HandleDrag(System.Windows.Point currentPos)
        {
            if (!isDragging || selectedElement == null)
                return false;
            Vector offset = currentPos - startPoint;
            Canvas.SetLeft(selectedElement, Canvas.GetLeft(selectedElement) + offset.X);
            Canvas.SetTop(selectedElement, Canvas.GetTop(selectedElement) + offset.Y);
            startPoint = currentPos;
            UpdateHighlightBorder(selectedElement);
            return true;
        }

        

        private void ResizeElement(UIElement element, Vector delta, ResizeDirection direction)
        {
            var fe = (FrameworkElement)element;
            double newWidth = fe.Width;
            double newHeight = fe.Height;
            double newLeft = Canvas.GetLeft(element);
            double newTop = Canvas.GetTop(element);
            switch (direction)
            {
                case ResizeDirection.Right:
                    newWidth += delta.X;
                    break;
                case ResizeDirection.Left:
                    newWidth -= delta.X;
                    newLeft += delta.X;
                    break;
                case ResizeDirection.Top:
                    newHeight -= delta.Y;
                    newTop += delta.Y;
                    break;
                case ResizeDirection.Bottom:
                    newHeight += delta.Y;
                    break;
                case ResizeDirection.TopLeft:
                    newWidth -= delta.X;
                    newLeft += delta.X;
                    newHeight -= delta.Y;
                    newTop += delta.Y;
                    break;
                case ResizeDirection.TopRight:
                    newWidth += delta.X;
                    newHeight -= delta.Y;
                    newTop += delta.Y;
                    break;
                case ResizeDirection.BottomLeft:
                    newWidth -= delta.X;
                    newLeft += delta.X;
                    newHeight += delta.Y;
                    break;
                case ResizeDirection.BottomRight:
                    newWidth += delta.X;
                    newHeight += delta.Y;
                    break;
            }

            if (newWidth > 10 && newHeight > 10)
            {
                fe.Width = newWidth;
                fe.Height = newHeight;
                Canvas.SetLeft(element, newLeft);
                Canvas.SetTop(element, newTop);
            }
        }

        private void ResizeElementCentered(UIElement element, System.Windows.Point currentPosition)
        {
            var fe = (FrameworkElement)element;

            double centerX = Canvas.GetLeft(fe) + fe.Width / 2;
            double centerY = Canvas.GetTop(fe) + fe.Height / 2;

            double distanceStart = (startPoint - new System.Windows.Point(centerX, centerY)).Length;
            double distanceCurrent = (currentPosition - new System.Windows.Point(centerX, centerY)).Length;

            double scale = distanceCurrent / distanceStart;

            if (scale <= 0.1) return;
            double newWidth = fe.Width * scale;
            double newHeight = fe.Height * scale;

            fe.Width = newWidth;
            fe.Height = newHeight;

            Canvas.SetLeft(fe, centerX - newWidth / 2);
            Canvas.SetTop(fe, centerY - newHeight / 2);

            startPoint = currentPosition;
        }

       

        private UIElement GetElementUnderMouse(System.Windows.Point position)
        {
            var hitResult = VisualTreeHelper.HitTest(MainCanvas, position);
            if (hitResult?.VisualHit is not DependencyObject hit)
                return null;

            DependencyObject current = hit;
            while (current != null && current != MainCanvas)
            {
                // Пропускаем highlightBorder
                if (current == highlightBorder)
                {
                    current = VisualTreeHelper.GetParent(current);
                    continue;
                }

                // Если нашли прямого ребёнка MainCanvas — это наш элемент
                if (VisualTreeHelper.GetParent(current) == MainCanvas)
                    return current as UIElement;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }




        private ResizeDirection GetResizeDirection(UIElement element, System.Windows.Point mouseCanvasPosition)
        {
            var fe = (FrameworkElement)element;
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);
            double right = left + fe.Width;
            double bottom = top + fe.Height;
            const double margin = 6;
            bool leftEdge = Math.Abs(mouseCanvasPosition.X - left) <= margin;
            bool rightEdge = Math.Abs(mouseCanvasPosition.X - right) <= margin;
            bool topEdge = Math.Abs(mouseCanvasPosition.Y - top) <= margin;
            bool bottomEdge = Math.Abs(mouseCanvasPosition.Y - bottom) <= margin;
            if (topEdge && leftEdge) return ResizeDirection.TopLeft;
            if (topEdge && rightEdge) return ResizeDirection.TopRight;
            if (bottomEdge && leftEdge) return ResizeDirection.BottomLeft;
            if (bottomEdge && rightEdge) return ResizeDirection.BottomRight;
            if (leftEdge) return ResizeDirection.Left;
            if (rightEdge) return ResizeDirection.Right;
            if (topEdge) return ResizeDirection.Top;
            if (bottomEdge) return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }


        private System.Windows.Input.Cursor GetCursorForDirection(ResizeDirection direction)
        {
            return direction switch
            {
                ResizeDirection.TopLeft or ResizeDirection.BottomRight => System.Windows.Input.Cursors.SizeNWSE,
                ResizeDirection.TopRight or ResizeDirection.BottomLeft => System.Windows.Input.Cursors.SizeNESW,
                ResizeDirection.Left or ResizeDirection.Right => System.Windows.Input.Cursors.SizeWE,
                ResizeDirection.Top or ResizeDirection.Bottom => System.Windows.Input.Cursors.SizeNS,
                _ => System.Windows.Input.Cursors.Arrow,
            };
        }

        

        private void ResizeContainer(UIElement element, Vector delta, ResizeDirection direction)
        {
            if (element is not FrameworkElement fe) return;

            double newW = fe.Width;
            double newH = fe.Height;
            double newLeft = Canvas.GetLeft(fe);
            double newTop = Canvas.GetTop(fe);

            switch (direction)
            {
                case ResizeDirection.Right:
                    newW += delta.X;
                    break;
                case ResizeDirection.Left:
                    newW -= delta.X;
                    newLeft += delta.X;
                    break;
                case ResizeDirection.Top:
                    newH -= delta.Y;
                    newTop += delta.Y;
                    break;
                case ResizeDirection.Bottom:
                    newH += delta.Y;
                    break;
                case ResizeDirection.TopLeft:
                    newW -= delta.X; newLeft += delta.X;
                    newH -= delta.Y; newTop += delta.Y;
                    break;
                case ResizeDirection.TopRight:
                    newW += delta.X;
                    newH -= delta.Y; newTop += delta.Y;
                    break;
                case ResizeDirection.BottomLeft:
                    newW -= delta.X; newLeft += delta.X;
                    newH += delta.Y;
                    break;
                case ResizeDirection.BottomRight:
                    newW += delta.X;
                    newH += delta.Y;
                    break;
                default:
                    return;
            }

            if (newW < 20 || newH < 20) return;

            fe.Width = newW;
            fe.Height = newH;
            Canvas.SetLeft(fe, newLeft);
            Canvas.SetTop(fe, newTop);
            RichTextBox FindWrappedText(Canvas wrapper)
            {
                return wrapper.Children.OfType<RichTextBox>().FirstOrDefault();
            }

            if (fe is Canvas wrapper && wrapper.Tag as string == "OptionWrapper")
            {
                var img = wrapper.FindWrappedImage();
                var rtb = wrapper.Children.OfType<RichTextBox>().FirstOrDefault();
                if (img != null && rtb != null)
                    LayoutWrappedControls(wrapper, img, rtb);
            }


        }

        private void ResizeContainerCentered(UIElement element, System.Windows.Point currentPos)
        {
            if (element is not FrameworkElement fe) return;
            double left = Canvas.GetLeft(fe);
            double top = Canvas.GetTop(fe);
            double centerX = left + fe.Width / 2;
            double centerY = top + fe.Height / 2;

            double startDist = (startPoint - new System.Windows.Point(centerX, centerY)).Length;
            double currDist = (currentPos - new System.Windows.Point(centerX, centerY)).Length;
            if (startDist < 0.1) return;
            double scale = Math.Max(0.1, currDist / startDist);

            double newW = fe.Width * scale;
            double newH = fe.Height * scale;
            if (newW < 20 || newH < 20) return;

            double newLeft = centerX - newW / 2;
            double newTop = centerY - newH / 2;

            fe.Width = newW;
            fe.Height = newH;
            Canvas.SetLeft(fe, newLeft);
            Canvas.SetTop(fe, newTop);

            if (fe is Canvas wrapper && wrapper.Tag as string == "OptionWrapper")
            {
                var img = wrapper.FindWrappedImage();
                if (img != null)
                {
                    img.Width = Math.Max(0, newW - 10);
                    img.Height = Math.Max(0, newH - 10);
                }
                CenterWrappedElements(wrapper);
            }

            startPoint = currentPos;
        }
        private void CenterWrappedElements(Canvas wrapper)
        {
            if (wrapper.Children.OfType<Viewbox>().FirstOrDefault() is Viewbox vb
                && vb.Child is Canvas content)
            {
                foreach (FrameworkElement child in content.Children)
                {
                    double left = (wrapper.Width - child.Width) / 2;
                    double top = (wrapper.Height - child.Height) / 2;

                    Canvas.SetLeft(child, left);
                    Canvas.SetTop(child, top);
                }
            }
        }

        private void UpdateHighlightBorder(UIElement element = null)
        {
            if ((element ?? selectedElement) == null || highlightBorder == null) return;
            var target = element ?? selectedElement;

            highlightBorder.Tag = target;

            highlightBorder.Width = ((FrameworkElement)target).Width;
            highlightBorder.Height = ((FrameworkElement)target).Height;
            Canvas.SetLeft(highlightBorder, Canvas.GetLeft(target));
            Canvas.SetTop(highlightBorder, Canvas.GetTop(target));
            highlightBorder.Visibility = Visibility.Visible;
        }


        private void AdjustCanvasSizeToImage()
        { 
            if (SupereventFrame.Source is BitmapSource original)
            {
                var formattedBitmap = new FormatConvertedBitmap(original, PixelFormats.Bgra32, null, 0);

                var fixedBitmap = BitmapSource.Create(
                    formattedBitmap.PixelWidth,
                    formattedBitmap.PixelHeight,
                    96,
                    96,
                    formattedBitmap.Format,
                    null,
                    GetPixels(formattedBitmap),
                    formattedBitmap.PixelWidth * (formattedBitmap.Format.BitsPerPixel / 8)
                );
                SupereventFrame.Source = fixedBitmap; 
                MainCanvas.Width = fixedBitmap.PixelWidth;
                MainCanvas.Height = fixedBitmap.PixelHeight;
            }
        }

     
        
        public MediaPlayer mediaPlayer = new MediaPlayer();
        public bool isPlaying = false;
        public string selectedAudioPath;
        public string currentTempAudioPath = null;
        

        private byte[] GetPixels(BitmapSource source)
        {
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[stride * source.PixelHeight];
            source.CopyPixels(pixels, stride, 0);
            return pixels;
        }


        private void LayoutWrappedControls(Canvas wrapper, System.Windows.Controls.Image img, RichTextBox rtb)
        {
            img.Width = wrapper.Width;
            img.Height = wrapper.Height;
            rtb.Padding = new Thickness(0);
            rtb.Document.PagePadding = new Thickness(0);
            rtb.Document.TextAlignment = TextAlignment.Center;
            rtb.Document.PageWidth = wrapper.Width;
            rtb.Measure(new System.Windows.Size(wrapper.Width, double.PositiveInfinity));
            var desired = rtb.DesiredSize;
            rtb.Width = wrapper.Width;
            rtb.Height = desired.Height;
            double left = (wrapper.Width - desired.Width) / 2;
            double top = (wrapper.Height - desired.Height) / 2;
            Canvas.SetLeft(rtb, left);
            Canvas.SetTop(rtb, top);
        }
        #endregion
        #region Events

        private void RichTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.RichTextBox rtb)
            {
                rtb.IsReadOnly = !rtb.IsReadOnly;

                if (!rtb.IsReadOnly)
                {
                    rtb.Focus();
                    rtb.CaretPosition = rtb.Document.ContentEnd;
                }
                else
                {
                    Keyboard.ClearFocus();
                }

            }
        }

        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = GetElementUnderMouse(e.GetPosition(MainCanvas));
            if (hit == null || hit == SupereventFrame)
                return;

            selectedElement = GetTopLevelChild(hit);

            isDragging = true;
            startPoint = e.GetPosition(MainCanvas);
            selectedElement.CaptureMouse();
            e.Handled = true;
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                return;

            if (IsInEditableRichTextBox(e))
                return;

            var hit = GetElementUnderMouse(e.GetPosition(MainCanvas));
            if (hit == null || hit == SupereventFrame)
            {
                selectedElement = null;
                return;
            }

            selectedElement = GetTopLevelChild(hit);
            startPoint = e.GetPosition(MainCanvas);
            elementBounds = new Rect(
                Canvas.GetLeft(selectedElement),
                Canvas.GetTop(selectedElement),
                ((FrameworkElement)selectedElement).Width,
                ((FrameworkElement)selectedElement).Height);

            resizeDirection = GetResizeDirection(selectedElement, e.GetPosition(MainCanvas));
            if (resizeDirection != ResizeDirection.None)
            {
                isResizing = true;
                startPoint = e.GetPosition(MainCanvas);
                selectedElement.CaptureMouse();
                if (selectedElement is Canvas wrapper)
                {
                    originalSize = new System.Windows.Size(wrapper.Width, wrapper.Height);
                    originalChildBounds = wrapper.Children
                        .OfType<FrameworkElement>()
                        .ToDictionary(
                            ch => (UIElement)ch,
                            ch => new Rect(
                                Canvas.GetLeft(ch),
                                Canvas.GetTop(ch),
                                ch.Width,
                                ch.Height));
                }
            }

            e.Handled = true;
        }

        private void MainCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Arrow;
            isDragging = false;
            isResizing = false;
            resizeDirection = ResizeDirection.None;
            MainCanvas.ReleaseMouseCapture();
            highlightBorder.Visibility = Visibility.Collapsed;
        }
        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (IsInEditableRichTextBox(e))
                return;

            if (selectedElement != null && selectedElement != SupereventFrame)
            {
                selectedElement.ReleaseMouseCapture();
            }
            selectedElement = null;
            isDragging = false;
            isResizing = false;
            resizeDirection = ResizeDirection.None;
            Keyboard.ClearFocus();
        }

        private void FramePickerButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var newImageSource = new BitmapImage(new Uri(openFileDialog.FileName));
                    SupereventFrame.Source = newImageSource;
                    AdjustCanvasSizeToImage();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AudioPickerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Аудиофайлы (*.wav)|*.wav";

            if (dialog.ShowDialog() == true)
            {
                selectedAudioPath = dialog.FileName;
                System.Windows.MessageBox.Show($"Выбран файл: {System.IO.Path.GetFileName(selectedAudioPath)}");
            }
        }

        private void AudioPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var kak = MainCanvas;
            if (isPlaying)
            {
                mediaPlayer.Stop();
                isPlaying = false;
                button.Content = "▶";
                return;
            }

            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "data", "temp");
            Directory.CreateDirectory(tempDir);

            try
            {
                using (var reader = new MediaFoundationReader(selectedAudioPath))
                using (var pcmStream = new WaveFormatConversionStream(new WaveFormat(44100, 16, reader.WaveFormat.Channels), reader))
                using (var memoryStream = new MemoryStream())
                {
                    WaveFileWriter.WriteWavFileToStream(memoryStream, pcmStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    string tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".wav");
                    File.WriteAllBytes(tempFile, memoryStream.ToArray());
                    currentTempAudioPath = tempFile;
                    mediaPlayer.Open(new Uri(tempFile));
                    mediaPlayer.Play();
                    isPlaying = true;
                    button.Content = "⏹";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка при воспроизведении: " + ex.Message);
                button.Content = "▶";
            }
        }

        private void FontSizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox box && double.TryParse(box.Text, out double result))
            {
                string boxName = box.Name;

                if (!boxName.Contains("Option"))
                {
                    string targetTextBoxName = boxName switch
                    {
                        "HeaderSizeBox" => "HeaderTextLocalizedField",
                        "DescSizeBox" => "DescTextLocalizedField",
                        _ => null
                    };

                    if (!string.IsNullOrEmpty(targetTextBoxName))
                    {
                        var target = MainCanvas.FindChildByName<RichTextBox>(targetTextBoxName);
                        if (target != null)
                        {
                            target.FontSize = result;
                        }
                    }
                }
                else
                {
                    List<Canvas> buttons = MainCanvas.FindElementsOfType<Canvas>();
                    foreach (Canvas button in buttons)
                    {
                        var rtb = button.FindElementsOfType<RichTextBox>().FirstOrDefault();
                        if (rtb != null)
                        {
                            rtb.FontSize = result;
                        }
                    }
                }
            }
        }
        private void ImageDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];
                    var imageControl = sender as System.Windows.Controls.Image;

                    try
                    {
                        BitmapImage original = new BitmapImage();
                        original.BeginInit();
                        original.UriSource = new Uri(imagePath);
                        original.CacheOption = BitmapCacheOption.OnLoad;
                        original.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        original.EndInit();
                        original.Freeze(); // безопасно для потоков
                        var formattedBitmap = new FormatConvertedBitmap(original, PixelFormats.Bgra32, null, 0);
                        var dpiNormalized = BitmapSource.Create(
                            formattedBitmap.PixelWidth,
                            formattedBitmap.PixelHeight,
                            96, // DPI X
                            96, // DPI Y
                            formattedBitmap.Format,
                            null,
                            GetPixels(formattedBitmap),
                            formattedBitmap.PixelWidth * (formattedBitmap.Format.BitsPerPixel / 8)
                        );
                        dpiNormalized.Freeze();

                        if (imageControl != null)
                        {
                            imageControl.Source = dpiNormalized;
                            imageControl.Width = dpiNormalized.PixelWidth;
                            imageControl.Height = dpiNormalized.PixelHeight;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                }
            }
        }
        private void UpdateConfigButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentConfig.Header = new TextRange(
                HeaderTextLocalizedField.Document.ContentStart,
                HeaderTextLocalizedField.Document.ContentEnd).Text.TrimEnd('\r', '\n');

            CurrentConfig.Description = new TextRange(
                DescTextLocalizedField.Document.ContentStart,
                DescTextLocalizedField.Document.ContentEnd).Text.TrimEnd('\r', '\n');

            CurrentConfig.SoundPath = selectedAudioPath;
            CurrentConfig.Id = SuperEventIdBox.Text;

            CurrentConfig.EventConstructor = MainCanvas;
        }
        private void DoneConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(CurrentConfig.Id))
            {
                UpdateConfigButton_Click(sender, e);
                SuperEventHandler handler = new() { CurrentConfig = CurrentConfig };
                handler.HandleFontFiles();
                handler.SaveSupereventAudioFile();
                handler.CreateCustomAssetsFiles();
                handler.SaveSupereventImages();
                handler.HandleGUIFile();
                handler.HandleGFXFile();
                handler.HandleScriptedGuiFile();
                handler.HandleLocalizationFiles();
                handler.HandleButtonsImage();
                handler.HandleFontDefineFiles();
            }
            else
            {
                System.Windows.MessageBox.Show("Заполните все необходимые поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void AddOptionButtonEvent()
        {
            char nextLetter = (char)('A' + MainCanvas.Children.OfType<Canvas>()
                .Count(c => c.Name?.StartsWith("OptionButton") == true));
            if (nextLetter > 'Z') return;
            string nameSuffix = nextLetter.ToString();
            var wrapper = new Canvas
            {
                Name = "OptionButton" + nameSuffix,
                Width = 110,
                Height = 30,
                Background = System.Windows.Media.Brushes.Transparent,
                AllowDrop = true,
                Tag = "OptionWrapper"
            };
            var optionImage = new System.Windows.Controls.Image
            {
                Name = "OptionImage" + nameSuffix,
                Stretch = Stretch.Fill,
                Tag = "OptionElement",
                Source = Imaging.CreateBitmapSourceFromHBitmap(
                    Properties.Resources.null_item_image.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                )
            };
            optionImage.Width = wrapper.Width;
            optionImage.Height = wrapper.Height;
            Canvas.SetLeft(optionImage, 0);
            Canvas.SetTop(optionImage, 0);
            var optionText = new RichTextBox
            {
                Name = "OptionText" + nameSuffix,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                Tag = "OptionElement"
            };
            optionText.Document.Blocks.Clear();
            optionText.Document.Blocks.Add(new Paragraph(new Run("test")));

            wrapper.Children.Add(optionImage);
            wrapper.Children.Add(optionText);

            wrapper.PreviewDragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                    e.Effects = System.Windows.DragDropEffects.Copy;
                else
                    e.Effects = System.Windows.DragDropEffects.None;
                e.Handled = true;
            };
            wrapper.PreviewDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    ImageDrop(optionImage, e);
                    wrapper.Width = optionImage.Source.Width;
                    wrapper.Height = optionImage.Source.Height;
                    LayoutWrappedControls(wrapper, optionImage, optionText);
                    e.Handled = true;
                }
            };

            LayoutWrappedControls(wrapper, optionImage, optionText);

            wrapper.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    RichTextBox_MouseDoubleClick(optionText, e);
                    e.Handled = true;
                    return;
                }
                MainCanvas_MouseLeftButtonDown(wrapper, e);
                e.Handled = true;
            };
            wrapper.PreviewMouseRightButtonDown += (s, e) =>
            {
                MainCanvas_MouseRightButtonDown(wrapper, e);
                e.Handled = true;
            };
            wrapper.MouseMove += (s, e) =>
            {
                MainCanvas_MouseMove(wrapper, e);
                e.Handled = true;
            };
            wrapper.MouseUp += (s, e) =>
            {
                MainCanvas_MouseUp(wrapper, e);
                e.Handled = true;
            };

            Canvas.SetLeft(wrapper, 20 + (nextLetter - 'A') * 120);
            Canvas.SetTop(wrapper, 20);
            MainCanvas.Children.Add(wrapper);
            Dispatcher.BeginInvoke(new Action(() => UpdateHighlightBorder(wrapper)), DispatcherPriority.Render);
        }
        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            AddOptionButtonEvent();
        }
        private void OptionSizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(((TextBox)sender)?.Text, out double newSize))
                return;

            var wrappers = MainCanvas
                .FindElementsOfType<Canvas>()
                .Where(c => c.Tag as string == "OptionWrapper");

            foreach (var wrapper in wrappers)
            {
                var rtb = wrapper.Children.OfType<RichTextBox>().FirstOrDefault();
                var img = wrapper.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
                if (rtb == null || img == null) continue;

                rtb.FontSize = newSize;
                rtb.Width = wrapper.Width;
                string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text
                                    .TrimEnd('\r', '\n');
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                rtb.Height = lines.Length * newSize * 1.2;

                Canvas.SetLeft(rtb, 0);
                Canvas.SetTop(rtb, Math.Max(0, (wrapper.Height - rtb.Height) / 2));

                img.Width = wrapper.Width;
                img.Height = wrapper.Height;
                Canvas.SetLeft(img, 0);
                Canvas.SetTop(img, 0);
            }
        }

        private void SearchableCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is SearchableComboBoxControl combo)
            {
                var fonts = Fonts.SystemFontFamilies
                                 .Select(f => f.Source)
                                 .ToList();

                combo.ItemsSource = fonts;

                string currentFont = combo.FontFamily.Source;
                if (fonts.Contains(currentFont))
                {
                    combo.SelectedItem = currentFont;
                }
                else
                {
                    combo.SelectedItem = null;
                }
            }
        }



        private void SearchableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox combo)
            {
                Debugger.Instance.LogMessage("SearchableCombo_SelectionChanged");
                string comboName = combo.Name;

                string fontName = combo.SelectedItem as string;
                if (string.IsNullOrEmpty(fontName))
                    return;

                var newFont = new FontFamily(fontName);
                string targetTextBoxName = null;

                if (!comboName.Contains("Option"))
                {
                    targetTextBoxName = comboName switch
                    {
                        "HeaderFontPickerComboBox" => "HeaderTextLocalizedField",
                        "DescFontPickerComboBox" => "DescTextLocalizedField",
                        _ => null
                    };

                    if (!string.IsNullOrEmpty(targetTextBoxName))
                    {
                        var target = MainCanvas.FindChildByName<RichTextBox>(targetTextBoxName);
                        if (target != null)
                            target.FontFamily = newFont;
                    }
                }
                else
                {
                    var buttons = MainCanvas.FindElementsOfType<Canvas>();
                    foreach (Canvas button in buttons)
                    {
                        var rtb = button.FindElementsOfType<RichTextBox>().FirstOrDefault();
                        if (rtb != null)
                            rtb.FontFamily = newFont;
                    }
                }
                Debugger.Instance.LogMessage("fin");
            }
        }


        private void FontColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Xceed.Wpf.Toolkit.ColorPicker picker)
            {
                string pickerName = picker.Name;

                string targetTextBoxName = pickerName switch
                {
                    "HeaderFontColorPicker" => "HeaderTextLocalizedField",
                    "DescFontColorPicker" => "DescTextLocalizedField",
                    _ => null
                };

                if (!string.IsNullOrEmpty(targetTextBoxName))
                {
                    var target = MainCanvas.FindChildByName<RichTextBox>(targetTextBoxName);
                    if (target != null && target.Foreground is SolidColorBrush brush)
                    {
                        picker.SelectedColor = brush.Color;
                    }
                }
            }
        }

        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsInEditableRichTextBox(e))
                return;

            System.Windows.Point currentPos = e.GetPosition(MainCanvas);

            if (SupereventFrame != null && SupereventFrame.IsMouseOver)
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
                highlightBorder.Visibility = Visibility.Collapsed;
                return;
            }

            if (HandleResize(currentPos) || HandleDrag(currentPos))
                return;

            var element = GetElementUnderMouse(currentPos);
            if (element != null && element != SupereventFrame)
            {

                var dir = GetResizeDirection(element, e.GetPosition(MainCanvas));
                if (dir != ResizeDirection.None)
                {
                    Cursor = (dir == ResizeDirection.None)
                   ? System.Windows.Input.Cursors.Arrow
                   : GetCursorForDirection(dir);

                    UpdateHighlightBorder(element);
                }
            }
            else
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
                highlightBorder.Visibility = Visibility.Collapsed;
            }
        }
        private void FontSizeBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox sizeBox)
            {
                // Получаем имя TextBox
                string boxName = sizeBox.Name;

                // Определяем, к какому RichTextBox он относится
                string targetTextBoxName = boxName switch
                {
                    "HeaderSizeBox" => "HeaderTextLocalizedField",
                    "DescSizeBox" => "DescTextLocalizedField",
                    _ => null
                };

                if (!string.IsNullOrEmpty(targetTextBoxName))
                {
                    var target = MainCanvas.FindChildByName<RichTextBox>(targetTextBoxName);
                    if (target != null)
                    {
                        // Получаем текущий размер шрифта
                        var fontSize = target.FontSize;
                        sizeBox.Text = fontSize.ToString("0.#"); // Без лишних знаков после запятой
                    }
                }
            }
        }
        private void FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (sender is Xceed.Wpf.Toolkit.ColorPicker picker)
            {
                string pickerName = picker.Name;

                if (!pickerName.Contains("Option"))
                {
                    string targetTextBoxName = pickerName switch
                    {
                        "HeaderFontColorPicker" => "HeaderTextLocalizedField",
                        "DescFontColorPicker" => "DescTextLocalizedField",
                        _ => null
                    };

                    if (!string.IsNullOrEmpty(targetTextBoxName))
                    {
                        var target = MainCanvas.FindChildByName<RichTextBox>(targetTextBoxName);
                        if (target != null)
                        {
                            target.Foreground = picker.SelectedColor.HasValue
                                ? new SolidColorBrush(picker.SelectedColor.Value)
                                : Brushes.Transparent;
                        }
                    }
                }
                else
                {
                    List<Canvas> buttons = MainCanvas.FindElementsOfType<Canvas>();
                    foreach (Canvas button in buttons)
                    {
                        var rtb = button.FindElementsOfType<RichTextBox>().FirstOrDefault();
                        if (rtb != null)
                        {
                            rtb.Foreground = picker.SelectedColor.HasValue
                                ? new SolidColorBrush(picker.SelectedColor.Value)
                                : Brushes.Transparent;
                        }
                    }
                }
            }
        }


        #endregion

    }
}
