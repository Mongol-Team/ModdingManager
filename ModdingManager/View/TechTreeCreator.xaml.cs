using ModdingManager.Controls;
using ModdingManager.Intefaces;
using ModdingManager.Presenters;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace ModdingManager
{
    public partial class TechTreeCreator : Window, ITechTreeCreatorView
    {
        public TechTreeCreator()
        {
            InitializeComponent();
            TechTreeWorkerPresenter presenter = new TechTreeWorkerPresenter(this);
        }

        #region Реализация интерфейса ITechTreeCreatorView

        public event EventHandler AddItemRequested;
        public event EventHandler UpdateItemRequested;
        public event EventHandler AddChildConnectionRequested;
        public event EventHandler AddMutualConnectionRequested;
        public event EventHandler SaveTreeRequested;
        public event EventHandler LoadTreeRequested;
        public event EventHandler GenerateTreeRequested;
        public event EventHandler DoneRequested;
        public event EventHandler DebugRequested;
        public event EventHandler<TechTreeItemConfig> GridElementEditRequested;
        public event EventHandler<TechTreeItemConfig> SetItemToControlsRequested;
        public event EventHandler<TechTreeItemConfig> UpdateItemToControlsRequested;
        public event EventHandler ClearControlsRequested;

        private TechTreeConfig _techTreeConfig;
        public TechTreeConfig TechTreeConfig
        {
            get => TechGrid.GetTree();
            set
            {
                _techTreeConfig = value;
                TechGrid.SetTree(value);
                UpdateUIFromConfig(value);
            }
        }
        public TechnologyGrid TechnologyGrid
        {
            get => TechGrid;
            set
            {
                if (value != null)
                {
                    TechGrid = value;
                    TechGrid.ItemEdited += TechGrid_EditRequested;
                }
            }
        }
        public string TechId
        {
            get => TechIdBox.Text;
            set => TechIdBox.Text = value;
        }

        public string TechName
        {
            get => TechNameBox.Text;
            set => TechNameBox.Text = value;
        }

        public string TechDescription
        {
            get => TechDescBox.Text;
            set => TechDescBox.Text = value;
        }

        public int TechModifCost
        {
            get => int.TryParse(TechCostModifBox.Text, out int result) ? result : 0;
            set => TechCostModifBox.Text = value.ToString();
        }

        public int TechCost
        {
            get => int.TryParse(TechCostBox.Text, out int result) ? result : 0;
            set => TechCostBox.Text = value.ToString();
        }

        public int StartYear
        {
            get => int.TryParse(StartYeatBox.Text, out int result) ? result : 0;
            set => StartYeatBox.Text = value.ToString();
        }

        public string TreeName
        {
            get => TechTreeNameBox.Text;
            set => TechTreeNameBox.Text = value;
        }

        public string TreeOrientation
        {
            get => (OrientationBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            set
            {
                foreach (ComboBoxItem item in OrientationBox.Items)
                {
                    if (item.Content.ToString() == value)
                    {
                        OrientationBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        public string TreeLedger
        {
            get => TreeLedgerBox.Text;
            set => TreeLedgerBox.Text = value;
        }

        public List<string> Enables
        {
            get => GetRichTextBoxLines(EnablesBox);
            set => SetRichTextBoxLines(EnablesBox, value);
        }

        public List<string> Allowed
        {
            get => GetRichTextBoxLines(AllowedBox);
            set => SetRichTextBoxLines(AllowedBox, value);
        }

        public List<string> Effects
        {
            get => GetRichTextBoxLines(EffectBox);
            set => SetRichTextBoxLines(EffectBox, value);
        }

        public List<string> Modifiers
        {
            get => GetRichTextBoxLines(ModdifierBox);
            set => SetRichTextBoxLines(ModdifierBox, value);
        }

        public string AiWillDo
        {
            get => GetRichTextBoxText(AiWillDoBox);
            set
            {
                AiWillDoBox.Document.Blocks.Clear();
                AiWillDoBox.AppendText(value);
            }
        }

        public List<string> Dependencies
        {
            get => GetRichTextBoxLines(DependenciesBox);
            set => SetRichTextBoxLines(DependenciesBox, value);
        }

        public string Categories
        {
            get => GetRichTextBoxText(CathegoryModdifierBox);
            set
            {
                CathegoryModdifierBox.Document.Blocks.Clear();
                CathegoryModdifierBox.AppendText(value);
            }
        }

        public System.Drawing.Image SmallTechImage
        {
            get => SmallOverlayImage.Source?.ToBitmap();
            set => SmallOverlayImage.Source = value?.ToImageSource();
        }

        public System.Drawing.Image BigTechImage
        {
            get => BigOverlayImage.Source?.ToBitmap();
            set => BigOverlayImage.Source = value?.ToImageSource();
        }

        public bool IsBigImage => BigOverlayImage.Source != null;

        public void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void UpdateUIFromConfig(TechTreeConfig config)
        {
            if (config == null) return;

            TreeName = config.Id.AsString();
            TreeOrientation = config.Orientation;
            TreeLedger = config.Ledger;
        }

        public void ClearForm()
        {
            TechId = string.Empty;
            TechName = string.Empty;
            TechDescription = string.Empty;
            TechModifCost = 0;
            TechCost = 0;
            StartYear = 0;
            Enables = new List<string>();
            Allowed = new List<string>();
            Effects = new List<string>();
            Modifiers = new List<string>();
            AiWillDo = string.Empty;
            Dependencies = new List<string>();
            Categories = string.Empty;
            SmallTechImage = null;
            BigTechImage = null;
        }

        public void RefreshTechTreeView()
        {
            TechGrid.RefreshView();
        }

        #endregion

        #region Обработчики событий

        private void TechGrid_EditRequested(object sender, TechTreeItemConfig item)
        {
            GridElementEditRequested?.Invoke(this, item);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateItemRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ChildEvent(object sender, RoutedEventArgs e)
        {
            AddChildConnectionRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MutalEvent(object sender, RoutedEventArgs e)
        {
            AddMutualConnectionRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadEvent(object sender, RoutedEventArgs e)
        {
            LoadTreeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SaveEvent(object sender, RoutedEventArgs e)
        {
            SaveTreeRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            DoneRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            DebugRequested?.Invoke(this, EventArgs.Empty);
        }

        private void TechIconCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));

                    if (sender == SmallTechIconCanvas)
                    {
                        SmallOverlayImage.Source = bitmap;
                        BigOverlayImage.Source = null;
                    }
                    else if (sender == BigTechIconCanvas)
                    {
                        BigOverlayImage.Source = bitmap;
                        SmallOverlayImage.Source = null;
                    }
                }
            }
        }
        private void TabFolderImageCanvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];

                    try
                    {
                        BitmapImage original = new BitmapImage(new Uri(imagePath));

                        var resizedFirst = original.ResizeToBitmap(25, 25);
                        TabFolderFirstImage.Source = resizedFirst;

                        var resizedSecond = original.ResizeToBitmap(25, 25);
                        TabFolderSecondImage.Source = resizedSecond;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                }
            }
        }
        private void RemoveImageEvent(object sender, MouseButtonEventArgs e)
        {
            if (sender == SmallTechIconCanvas)
            {
                SmallOverlayImage.Source = null;
            }
            else if (sender == BigTechIconCanvas)
            {
                BigOverlayImage.Source = null;
            }
        }

        #endregion

        #region Вспомогательные методы

        private List<string> GetRichTextBoxLines(RichTextBox richTextBox)
        {
            var lines = new List<string>();
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string text = textRange.Text;

            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        lines.Add(line.Trim());
                }
            }

            return lines;
        }

        private void SetRichTextBoxLines(RichTextBox richTextBox, List<string> lines)
        {
            richTextBox.Document.Blocks.Clear();
            if (lines == null || lines.Count == 0) return;

            var paragraph = new Paragraph();
            foreach (string line in lines)
            {
                paragraph.Inlines.Add(new Run(line));
                paragraph.Inlines.Add(new LineBreak());
            }

            richTextBox.Document.Blocks.Add(paragraph);
        }

        private string GetRichTextBoxText(RichTextBox richTextBox)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return textRange.Text.Trim();
        }

        #endregion
    }
}