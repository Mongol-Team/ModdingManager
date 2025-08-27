using ModdingManager.Controls;
using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms.Integration;
using MessageBox = System.Windows.Forms.MessageBox;
namespace ModdingManager
{
    public partial class MainForm : Form
    {
        private bool _isLoaded = false;
        public MainForm()
        {
            InitializeComponent();
        }

        private void LocConvertButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                TreeLoc treeLoc = new TreeLoc();
                treeLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }



        private void Statebutton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                MapWorkerWindow MapWorkerWindow = new MapWorkerWindow();
                ElementHost.EnableModelessKeyboardInterop(MapWorkerWindow);
                MapWorkerWindow.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void LocTechButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void LocIdeaButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.GameDirectory = GameDirBox.Text;
                ModManager.ModDirectory = DirBox.Text;
                IdeaLoc ideaLoc = new IdeaLoc();
                ideaLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void LocStateButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.GameDirectory = GameDirBox.Text;
                ModManager.ModDirectory = DirBox.Text;
                StateLoc stateLoc = new StateLoc();
                stateLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void TechButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                TechTreeCreator fc = new TechTreeCreator();
                ElementHost.EnableModelessKeyboardInterop(fc);
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void FlagCrtButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                FlagCreator fc = new FlagCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void CountryCrtButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                WPFCountryCreator fc = new WPFCountryCreator();
                ElementHost.EnableModelessKeyboardInterop(fc);
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void DirBox_TextChanged(object sender, KeyEventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string relativePath = Path.Combine("..", "..", "..", "data", "dir.json");
            string fullPath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                string json = File.ReadAllText(fullPath);
                var path = JsonSerializer.Deserialize<PathConfig>(json);
                GameDirBox.Text = path.GamePath;
                DirBox.Text = path.ModPath;
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                Registry.LoadInstance();
                Logger.AddLog(Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace") + Directory.Exists(Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")));
                Logger.AddLog(Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage) + Directory.Exists(Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage)));
                Logger.AddLog(Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage) + Directory.Exists(Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage)));
                Logger.AddLog(Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace") + Directory.Exists(Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace")));

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.AddLog($"[MAIN Form] On load exeption :{ex.Message + ex.StackTrace}");
            }

        }

        private void CharCreator_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                CharacterCreator fc = new CharacterCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void IdeaCreatorButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                IdeaCreator fc = new IdeaCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void ModButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                ModifierCreator fc = new ModifierCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                TemplateCreator fc = new TemplateCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void SuperEventCreatorButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                SupereventCreator fc = new SupereventCreator();
                ElementHost.EnableModelessKeyboardInterop(fc);
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void DebugButton_Click(object sender, EventArgs e)
        {
            ModManager.IsDebugRuning = true;


            // Пример записи отладочного сообщения
            Logger.AddLog("Режим отладки активирован");

          
        }


        private void IdeologyCreatorBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.ModDirectory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                IdeologyCreator fc = new IdeologyCreator();
                ElementHost.EnableModelessKeyboardInterop(fc);
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void DirBox_TextChanged(object sender, EventArgs e)
        {
            if (_isLoaded == true)
            {
                if (!(string.IsNullOrEmpty(GameDirBox.Text) || string.IsNullOrEmpty(DirBox.Text)))
                {
                    string relativePath = Path.Combine("..", "..", "..", "data", "dir.json");
                    string fullPath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);
                    PathConfig config = new()
                    {
                        GamePath = GameDirBox.Text,
                        ModPath = DirBox.Text,
                    };
                    var json = JsonSerializer.Serialize(config);
                    File.WriteAllText(fullPath, json);
                }
                else
                {
                    MessageBox.Show("Укажите обе директории.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ErrorPanel errorPanel = new ErrorPanel();
            errorPanel.AddError(ErrorType.Warning, "This feature is not implemented yet.", "MapHealerWindow");
            errorPanel.AddError(ErrorType.Critical, "Пенис 24 вап ПРОСТО Я вапвап вап.", "C:\\Users\\Acer\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\history\\states\\1-France.txt");
            var dialog = new Window
            {
                Title = "Ошибки",
                Width = 475,
                Height = 338,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = errorPanel
            };

            dialog.Show();
        }
    }
}
