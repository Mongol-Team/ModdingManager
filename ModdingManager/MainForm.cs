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
            TreeLoc treeLoc = new TreeLoc();
            treeLoc.Show();
        }



        private void Statebutton_Click(object sender, EventArgs e)
        {

        }

        private void LocTechButton_Click(object sender, EventArgs e)
        {

        }

        private void LocIdeaButton_Click(object sender, EventArgs e)
        {
            IdeaLoc ideaLoc = new IdeaLoc();
            ideaLoc.Show();
        }

        private void LocStateButton_Click(object sender, EventArgs e)
        {
            StateLoc stateLoc = new StateLoc();
            stateLoc.Show();
        }

        private void TechButton_Click(object sender, EventArgs e)
        {

        }

        private void FlagCrtButton_Click(object sender, EventArgs e)
        {
            FlagCreator fc = new FlagCreator();
            fc.Show();
        }
    }
}
