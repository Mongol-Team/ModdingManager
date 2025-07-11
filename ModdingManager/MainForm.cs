using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using ModdingManager.configs;
using System.Drawing;
using System.Windows.Forms.Integration;
using ModdingManager.managers.utils;
using ModdingManager.classes.utils;
namespace ModdingManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

        }

        private void LocConvertButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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

        private void DirBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
                ModManager.Directory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                Registry.LoadInstance();
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"[MAIN Form] On load exeption :{ex.Message + ex.StackTrace}");
            }

        }

        private void CharCreator_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
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
                ModManager.Directory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                TemplateCreator fc = new TemplateCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void GameDirBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void SuperEventCreatorButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
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
            DebugWindow debugWindow = new DebugWindow();
            // Инициализируем Debugger (если используется Singleton, то Instance уже создан)
            Debugger.Instance.DebugOutputControl = debugWindow.DebugBox; // Подключаем RichTextBox для вывода логов

            // Подключаем текущее окно к Debugger для перехвата исключений
            Debugger.Instance.AttachToWindow(this);

            // Пример записи отладочного сообщения
            Debugger.Instance.LogMessage("Режим отладки активирован");

            // Можно сразу проверить работу на тестовом исключении
            try
            {
                throw new InvalidOperationException("Тестовое исключение для отладки");
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"Поймано исключение: {ex.Message}");
            }
            debugWindow.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Простое тестовое исключение
                throw new InvalidOperationException("Это тестовое исключение из button1_Click");

                // 2. Альтернативные варианты тестовых исключений (раскомментируйте для проверки):
                // throw new ArgumentNullException("testParameter", "Параметр не может быть null");
                // throw new IndexOutOfRangeException("Выход за границы массива");
                // throw new FileNotFoundException("Файл не найден", "example.txt");
            }
            catch (Exception ex)
            {
                // Записываем исключение в Debugger
                Debugger.Instance.LogMessage($"Поймано исключение в button1_Click: {ex}");


            }
        }

        private void IdeologyCreatorBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
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
    }
}
