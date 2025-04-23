using ModdingManager;

namespace ModdingManager
{
    partial class MainForm : Form
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            LocConvertButton = new Button();
            Statebutton = new Button();
            LocTechButton = new Button();
            ModButton = new Button();
            TechButton = new Button();
            LocStateButton = new Button();
            LocIdeaButton = new Button();
            FlagCrtButton = new Button();
            CountryCrtButton = new Button();
            DirBox = new TextBox();
            label1 = new Label();
            CharCreator = new Button();
            IdeaCreatorButton = new Button();
            TestButton = new Button();
            GameDirBox = new TextBox();
            label2 = new Label();
            SuspendLayout();
            // 
            // LocConvertButton
            // 
            LocConvertButton.BackColor = Color.FromArgb(0, 192, 0);
            LocConvertButton.Location = new Point(44, 56);
            LocConvertButton.Name = "LocConvertButton";
            LocConvertButton.Size = new Size(235, 56);
            LocConvertButton.TabIndex = 0;
            LocConvertButton.Text = "Локализация по дерву фокусов";
            LocConvertButton.UseVisualStyleBackColor = false;
            LocConvertButton.Click += LocConvertButton_Click;
            // 
            // Statebutton
            // 
            Statebutton.Location = new Point(361, 56);
            Statebutton.Name = "Statebutton";
            Statebutton.Size = new Size(237, 56);
            Statebutton.TabIndex = 1;
            Statebutton.Text = "Работа со стейтами";
            Statebutton.UseVisualStyleBackColor = true;
            Statebutton.Click += Statebutton_Click;
            // 
            // LocTechButton
            // 
            LocTechButton.Location = new Point(44, 134);
            LocTechButton.Name = "LocTechButton";
            LocTechButton.Size = new Size(235, 56);
            LocTechButton.TabIndex = 2;
            LocTechButton.Text = "Локализация по файлу технологии";
            LocTechButton.UseVisualStyleBackColor = true;
            LocTechButton.Click += LocTechButton_Click;
            // 
            // ModButton
            // 
            ModButton.Location = new Point(361, 134);
            ModButton.Name = "ModButton";
            ModButton.Size = new Size(237, 56);
            ModButton.TabIndex = 3;
            ModButton.Text = "Создатель модификаторов";
            ModButton.UseVisualStyleBackColor = true;
            ModButton.Click += ModButton_Click;
            // 
            // TechButton
            // 
            TechButton.BackColor = Color.FromArgb(0, 192, 192);
            TechButton.Location = new Point(361, 210);
            TechButton.Name = "TechButton";
            TechButton.Size = new Size(237, 56);
            TechButton.TabIndex = 4;
            TechButton.Text = "Создатель технологий";
            TechButton.UseVisualStyleBackColor = false;
            TechButton.Click += TechButton_Click;
            // 
            // LocStateButton
            // 
            LocStateButton.BackColor = Color.FromArgb(0, 192, 0);
            LocStateButton.Location = new Point(44, 210);
            LocStateButton.Name = "LocStateButton";
            LocStateButton.Size = new Size(235, 56);
            LocStateButton.TabIndex = 5;
            LocStateButton.Text = "Локализация по файлу стейта";
            LocStateButton.UseVisualStyleBackColor = false;
            LocStateButton.Click += LocStateButton_Click;
            // 
            // LocIdeaButton
            // 
            LocIdeaButton.BackColor = Color.FromArgb(0, 192, 0);
            LocIdeaButton.Location = new Point(44, 294);
            LocIdeaButton.Name = "LocIdeaButton";
            LocIdeaButton.Size = new Size(235, 56);
            LocIdeaButton.TabIndex = 6;
            LocIdeaButton.Text = "Локализация для идей";
            LocIdeaButton.UseVisualStyleBackColor = false;
            LocIdeaButton.Click += LocIdeaButton_Click;
            // 
            // FlagCrtButton
            // 
            FlagCrtButton.BackColor = Color.FromArgb(192, 192, 0);
            FlagCrtButton.Location = new Point(361, 294);
            FlagCrtButton.Name = "FlagCrtButton";
            FlagCrtButton.Size = new Size(237, 56);
            FlagCrtButton.TabIndex = 7;
            FlagCrtButton.Text = "Создатель флагов";
            FlagCrtButton.UseVisualStyleBackColor = false;
            FlagCrtButton.Click += FlagCrtButton_Click;
            // 
            // CountryCrtButton
            // 
            CountryCrtButton.BackColor = Color.FromArgb(0, 192, 0);
            CountryCrtButton.Location = new Point(684, 56);
            CountryCrtButton.Name = "CountryCrtButton";
            CountryCrtButton.Size = new Size(237, 56);
            CountryCrtButton.TabIndex = 8;
            CountryCrtButton.Text = "Создатель стран";
            CountryCrtButton.UseVisualStyleBackColor = false;
            CountryCrtButton.Click += CountryCrtButton_Click;
            // 
            // DirBox
            // 
            DirBox.Location = new Point(44, 408);
            DirBox.Name = "DirBox";
            DirBox.Size = new Size(554, 27);
            DirBox.TabIndex = 9;
            DirBox.KeyDown += DirBox_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(638, 415);
            label1.Name = "label1";
            label1.Size = new Size(146, 20);
            label1.TabIndex = 10;
            label1.Text = "Дирректория мода ";
            // 
            // CharCreator
            // 
            CharCreator.BackColor = Color.FromArgb(0, 192, 0);
            CharCreator.Location = new Point(684, 134);
            CharCreator.Name = "CharCreator";
            CharCreator.Size = new Size(237, 56);
            CharCreator.TabIndex = 11;
            CharCreator.Text = "Создатель персонажей";
            CharCreator.UseVisualStyleBackColor = false;
            CharCreator.Click += CharCreator_Click;
            // 
            // IdeaCreatorButton
            // 
            IdeaCreatorButton.BackColor = Color.FromArgb(0, 192, 0);
            IdeaCreatorButton.Location = new Point(684, 210);
            IdeaCreatorButton.Name = "IdeaCreatorButton";
            IdeaCreatorButton.Size = new Size(237, 56);
            IdeaCreatorButton.TabIndex = 12;
            IdeaCreatorButton.Text = "Создатель идей";
            IdeaCreatorButton.UseVisualStyleBackColor = false;
            IdeaCreatorButton.Click += IdeaCreatorButton_Click;
            // 
            // TestButton
            // 
            TestButton.Location = new Point(684, 294);
            TestButton.Name = "TestButton";
            TestButton.Size = new Size(237, 56);
            TestButton.TabIndex = 13;
            TestButton.Text = "Test";
            TestButton.UseVisualStyleBackColor = true;
            TestButton.Click += TestButton_Click;
            // 
            // GameDirBox
            // 
            GameDirBox.Location = new Point(44, 455);
            GameDirBox.Name = "GameDirBox";
            GameDirBox.Size = new Size(554, 27);
            GameDirBox.TabIndex = 14;
            GameDirBox.KeyDown += DirBox_KeyDown;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(638, 455);
            label2.Name = "label2";
            label2.Size = new Size(145, 20);
            label2.TabIndex = 15;
            label2.Text = "Дирректория игры ";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(963, 494);
            Controls.Add(label2);
            Controls.Add(GameDirBox);
            Controls.Add(TestButton);
            Controls.Add(IdeaCreatorButton);
            Controls.Add(CharCreator);
            Controls.Add(label1);
            Controls.Add(DirBox);
            Controls.Add(CountryCrtButton);
            Controls.Add(FlagCrtButton);
            Controls.Add(LocIdeaButton);
            Controls.Add(LocStateButton);
            Controls.Add(TechButton);
            Controls.Add(ModButton);
            Controls.Add(LocTechButton);
            Controls.Add(Statebutton);
            Controls.Add(LocConvertButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button LocConvertButton;
        private Button Statebutton;
        private Button LocTechButton;
        private Button ModButton;
        private Button TechButton;
        private Button LocStateButton;
        private Button LocIdeaButton;
        private Button FlagCrtButton;
        private Button CountryCrtButton;
        private TextBox DirBox;
        private Label label1;
        private Button CharCreator;
        private Button IdeaCreatorButton;
        private Button TestButton;
        private TextBox GameDirBox;
        private Label label2;
    }
}
