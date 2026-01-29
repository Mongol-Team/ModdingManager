using Application;
using Application.Extensions;
using Application.Extentions;
using Models.Configs;
using Models.Enums;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ViewControls;
using ViewInterfaces;
using ViewPresenters;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace View
{
    public partial class TechTreeCreator : BaseWindow, ITechTreeCreatorView
    {
        public TechTreeCreator()
        {
            InitializeComponent();
            TechTreeWorkerPresenter presenter = new(this);
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
        public Identifier TechId
        {
            get => new(TechIdBox.Text);
            set => TechIdBox.Text = value.ToString();
        }

        public ConfigLocalisation Localisation
        {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
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

        public TechTreeOrientationType TreeOrientation
        {
            get
            {
                if (OrientationBox.SelectedItem is ComboBoxItem item &&
                    Enum.TryParse<TechTreeOrientationType>(item.Content.ToString(), out var result))
                {
                    return result;
                }

                return default;
            }
            set
            {
                foreach (ComboBoxItem item in OrientationBox.Items)
                {
                    if (Enum.TryParse<TechTreeOrientationType>(item.Content.ToString(), out var parsed) &&
                        parsed.Equals(value))
                    {
                        OrientationBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }


        public TechTreeLedgerType TreeLedger
        {
            get
            {
                if (Enum.TryParse(TreeLedgerBox.Text, out TechTreeLedgerType result))
                    return result;
                else
                    return default;
            }
            set => TreeLedgerBox.Text = value.ToString();
        }




        public string Allowed
        {
            get => AllowedBox.GetRichTextBoxText();
            set => AllowedBox.SetRichTextBoxText(value);
        }

        public string Effects
        {
            get => EffectBox.GetRichTextBoxText();
            set => EffectBox.SetRichTextBoxText(value);
        }

        public Dictionary<ModifierDefinitionConfig, object> Modifiers
        {
            get
            {
                Dictionary<ModifierDefinitionConfig, object> mods = new();
                foreach (string modstr in ModdifierBox.GetLines())
                {
                    string[] splited = modstr.Split(':');
                    if (splited.Length != 2)
                    {
                        continue;
                    }
                    if (!double.TryParse(splited[1], out double value))
                    {
                        continue;
                    }
                    mods.Add(ModDataStorage.Mod.ModifierDefinitions.FindById(splited[0]), value);
                }
                return mods;
            }
            set
            {
                foreach (var kvp in value)
                {
                    ModdifierBox.AddLine($"{kvp.Key}:{kvp.Value}");
                }
            }
        }

        public string AiWillDo
        {
            get => AiWillDoBox.GetRichTextBoxText();
            set => AiWillDoBox.SetRichTextBoxText(value);

        }

        public Dictionary<TechTreeItemConfig, int> Dependencies
        {
            get
            {
                Dictionary<TechTreeItemConfig, int> deps = new();
                List<string> raw = DependenciesBox.GetRichTextBoxLines();
                raw.ForEach(line =>
                {
                    string[] splited = line.Split(':');
                    if (splited.Length != 2)
                    {
                        return;
                    }
                    int.TryParse(splited[1], out int value);
                    var tech = TechGrid.GetTree().Items.FindById(splited[0]);
                    if (tech == null || value == 0)
                    {
                        return;
                    }
                    deps.Add(tech, value);
                });
                return deps;
            }
            set
            {
                foreach (var kvp in value)
                {
                    DependenciesBox.AddLine($"{kvp.Key.Id.ToString()}:{kvp.Value}");
                }
            }
        }
        public List<TechCategoryConfig> Categories
        {
            get
            {
                return ModDataStorage.Mod.TechCategories
                .Where(cat => CathegoryModdifierBox.GetRichTextBoxLines()
                    .Any(line => cat.Id.ToString() == line))
                .ToList();
            }
            set
            {
                CathegoryModdifierBox.Document.Blocks.Clear();
                foreach (var cat in value)
                {
                    CathegoryModdifierBox.AddLine(cat.Id.ToString());
                }
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

        public Dictionary<BuildingConfig, object> EnableBuildings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<EquipmentConfig> EnableEquipments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<SubUnitConfig> EnableUnits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string AllowBranch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void ShowMessage(string message, string title)
        {
            ShowInfo(message, NotificationCorner.TopRight);
        }

        public void ShowError(string message, string title)
        {
            ShowError(message, NotificationCorner.TopRight);
        }

        public void UpdateUIFromConfig(TechTreeConfig config)
        {
            if (config == null) return;

            TreeName = config.Id.ToString();
            TreeOrientation = config.Orientation;
            TreeLedger = config.Ledger;
        }

        public void ClearForm()
        {
            TechId = new(string.Empty);
            TechModifCost = 0;
            TechCost = 0;
            StartYear = 0;
            Allowed = string.Empty;
            Effects = string.Empty;
            Modifiers = new Dictionary<ModifierDefinitionConfig, object>();
            AiWillDo = string.Empty;
            Dependencies = new Dictionary<TechTreeItemConfig, int>();
            Categories = new();
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
                    string FileFullPath = files[0];
                    BitmapImage bitmap = new(new Uri(FileFullPath));

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
                        BitmapImage original = new(new Uri(imagePath));

                        var resizedFirst = original.ResizeToBitmap(25, 25);
                        TabFolderFirstImage.Source = resizedFirst;

                        var resizedSecond = original.ResizeToBitmap(25, 25);
                        TabFolderSecondImage.Source = resizedSecond;
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Ошибка при загрузке изображения: {ex.Message}", NotificationCorner.TopRight);
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


    }
}