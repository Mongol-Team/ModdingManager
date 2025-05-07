using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using ModdingManager.managers;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using ModdingManager.configs;
using System.Drawing;
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
            }
        }

        private void TechButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                TechTreeCreator fc = new TechTreeCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
            }
        }

        private void CountryCrtButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text) || !string.IsNullOrEmpty(GameDirBox.Text))
            {
                ModManager.Directory = DirBox.Text;
                ModManager.GameDirectory = GameDirBox.Text;
                CountryCreator fc = new CountryCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                    MessageBox.Show("”кажите обе директории.", "ќшибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            }
            catch{}
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
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
                MessageBox.Show("¬ведите обе директории.", "ќшибка", MessageBoxButtons.OK);
            }
        }

        private void GameDirBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
