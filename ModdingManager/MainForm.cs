using static System.Runtime.InteropServices.JavaScript.JSType;

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
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                TreeLoc treeLoc = new TreeLoc();
                treeLoc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }



        private void Statebutton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void LocTechButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void LocIdeaButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                IdeaLoc ideaLoc = new IdeaLoc();
                ideaLoc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void LocStateButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                StateLoc stateLoc = new StateLoc();
                stateLoc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void TechButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void FlagCrtButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                FlagCreator fc = new FlagCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void CountryCrtButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                CountryCreator fc = new CountryCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }

        private void DirBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string relativePath = Path.Combine("..", "..", "..", "data", "dir.txt");
                string fullPath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);
                File.WriteAllText(fullPath, DirBox.Text);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string relativePath = Path.Combine("..", "..", "..", "data", "dir.txt");
            string fullPath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

            DirBox.Text = File.ReadAllText(fullPath);
        }

        private void CharCreator_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DirBox.Text))
            {
                ModManager.directory = DirBox.Text;
                CharacterCreator fc = new CharacterCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButtons.OK);
            }
        }
    }
}
