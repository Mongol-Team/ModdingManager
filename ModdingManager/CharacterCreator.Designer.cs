
namespace ModdingManager
{
    public partial class CharacterCreator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterCreator));
            BigIconPanel = new Panel();
            label1 = new Label();
            NameBox = new TextBox();
            label2 = new Label();
            DescBox = new TextBox();
            PercBox = new RichTextBox();
            label3 = new Label();
            IdBox = new TextBox();
            label8 = new Label();
            label4 = new Label();
            SkillBox = new TextBox();
            AtkBox = new TextBox();
            DefBox = new TextBox();
            SupplyBox = new TextBox();
            SpdBox = new TextBox();
            AdvisorCost = new TextBox();
            label5 = new Label();
            label6 = new Label();
            AdvisorSlot = new ComboBox();
            label7 = new Label();
            label9 = new Label();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            label13 = new Label();
            label14 = new Label();
            CharTypesBox = new RichTextBox();
            TagBox = new TextBox();
            label15 = new Label();
            SmalIconPanel = new Panel();
            AiDoBox = new TextBox();
            label16 = new Label();
            label17 = new Label();
            ExpireBox = new TextBox();
            ApplyButton = new Button();
            SaveButton = new Button();
            LoadButton = new Button();
            IdeologyBox = new TextBox();
            label18 = new Label();
            SuspendLayout();
            // 
            // BigIconPanel
            // 
            BigIconPanel.AllowDrop = true;
            BigIconPanel.BackColor = SystemColors.ActiveCaption;
            BigIconPanel.Location = new Point(29, 94);
            BigIconPanel.Name = "BigIconPanel";
            BigIconPanel.Size = new Size(156, 210);
            BigIconPanel.TabIndex = 0;
            BigIconPanel.DragDrop += BigIconBox_DragDrop;
            BigIconPanel.DragEnter += BigIconBox_DragEnter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(221, 145);
            label1.Name = "label1";
            label1.Size = new Size(75, 20);
            label1.TabIndex = 1;
            label1.Text = "Имя типа";
            // 
            // NameBox
            // 
            NameBox.Location = new Point(221, 168);
            NameBox.Name = "NameBox";
            NameBox.Size = new Size(166, 27);
            NameBox.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(221, 198);
            label2.Name = "label2";
            label2.Size = new Size(79, 20);
            label2.TabIndex = 3;
            label2.Text = "Описание";
            // 
            // DescBox
            // 
            DescBox.Location = new Point(221, 221);
            DescBox.Name = "DescBox";
            DescBox.Size = new Size(166, 27);
            DescBox.TabIndex = 4;
            // 
            // PercBox
            // 
            PercBox.Location = new Point(219, 330);
            PercBox.Name = "PercBox";
            PercBox.Size = new Size(166, 83);
            PercBox.TabIndex = 5;
            PercBox.Text = "";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(219, 307);
            label3.Name = "label3";
            label3.Size = new Size(53, 20);
            label3.TabIndex = 6;
            label3.Text = "Перки";
            // 
            // IdBox
            // 
            IdBox.Location = new Point(25, 57);
            IdBox.Name = "IdBox";
            IdBox.Size = new Size(166, 27);
            IdBox.TabIndex = 16;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(58, 21);
            label8.Name = "label8";
            label8.Size = new Size(107, 20);
            label8.TabIndex = 15;
            label8.Text = "Игровое айди";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(256, 21);
            label4.Name = "label4";
            label4.Size = new Size(80, 20);
            label4.TabIndex = 18;
            label4.Text = "Тип перса";
            // 
            // SkillBox
            // 
            SkillBox.Location = new Point(29, 430);
            SkillBox.Name = "SkillBox";
            SkillBox.Size = new Size(41, 27);
            SkillBox.TabIndex = 20;
            // 
            // AtkBox
            // 
            AtkBox.Location = new Point(29, 463);
            AtkBox.Name = "AtkBox";
            AtkBox.Size = new Size(41, 27);
            AtkBox.TabIndex = 21;
            // 
            // DefBox
            // 
            DefBox.Location = new Point(29, 496);
            DefBox.Name = "DefBox";
            DefBox.Size = new Size(41, 27);
            DefBox.TabIndex = 22;
            // 
            // SupplyBox
            // 
            SupplyBox.Location = new Point(29, 529);
            SupplyBox.Name = "SupplyBox";
            SupplyBox.Size = new Size(41, 27);
            SupplyBox.TabIndex = 23;
            // 
            // SpdBox
            // 
            SpdBox.Location = new Point(29, 562);
            SpdBox.Name = "SpdBox";
            SpdBox.Size = new Size(41, 27);
            SpdBox.TabIndex = 24;
            // 
            // AdvisorCost
            // 
            AdvisorCost.Location = new Point(219, 514);
            AdvisorCost.Name = "AdvisorCost";
            AdvisorCost.Size = new Size(166, 27);
            AdvisorCost.TabIndex = 25;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(219, 491);
            label5.Name = "label5";
            label5.Size = new Size(158, 20);
            label5.TabIndex = 26;
            label5.Text = "Стоимость советника";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(220, 544);
            label6.Name = "label6";
            label6.Size = new Size(116, 20);
            label6.TabIndex = 27;
            label6.Text = "Слот советника";
            // 
            // AdvisorSlot
            // 
            AdvisorSlot.FormattingEnabled = true;
            AdvisorSlot.Items.AddRange(new object[] { "political_advisor", "theorist", "army_chief", "navy_chief", "air_chief", "high_command" });
            AdvisorSlot.Location = new Point(220, 569);
            AdvisorSlot.Name = "AdvisorSlot";
            AdvisorSlot.Size = new Size(165, 28);
            AdvisorSlot.TabIndex = 28;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(76, 433);
            label7.Name = "label7";
            label7.Size = new Size(42, 20);
            label7.TabIndex = 29;
            label7.Text = "Скил";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(25, 408);
            label9.Name = "label9";
            label9.Size = new Size(129, 20);
            label9.TabIndex = 30;
            label9.Text = "Маршал-генерал";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(262, 471);
            label10.Name = "label10";
            label10.Size = new Size(74, 20);
            label10.TabIndex = 31;
            label10.Text = "Советник";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(81, 463);
            label11.Name = "label11";
            label11.Size = new Size(32, 20);
            label11.TabIndex = 32;
            label11.Text = "Атк";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(81, 499);
            label12.Name = "label12";
            label12.Size = new Size(37, 20);
            label12.TabIndex = 33;
            label12.Text = "Деф";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(76, 536);
            label13.Name = "label13";
            label13.Size = new Size(60, 20);
            label13.TabIndex = 34;
            label13.Text = "Саплай";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(76, 569);
            label14.Name = "label14";
            label14.Size = new Size(35, 20);
            label14.TabIndex = 35;
            label14.Text = "Спд";
            // 
            // CharTypesBox
            // 
            CharTypesBox.Location = new Point(211, 44);
            CharTypesBox.Name = "CharTypesBox";
            CharTypesBox.Size = new Size(166, 91);
            CharTypesBox.TabIndex = 36;
            CharTypesBox.Text = "advisor\nnavy_leader\nfield_marshal\ncorps_commander\ncountry_leader";
            // 
            // TagBox
            // 
            TagBox.Location = new Point(219, 277);
            TagBox.Name = "TagBox";
            TagBox.Size = new Size(166, 27);
            TagBox.TabIndex = 37;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(221, 254);
            label15.Name = "label15";
            label15.Size = new Size(160, 20);
            label15.TabIndex = 38;
            label15.Text = "Принадлежность тегу";
            // 
            // SmalIconPanel
            // 
            SmalIconPanel.AllowDrop = true;
            SmalIconPanel.BackColor = SystemColors.ActiveCaption;
            SmalIconPanel.Location = new Point(29, 330);
            SmalIconPanel.Name = "SmalIconPanel";
            SmalIconPanel.Size = new Size(67, 65);
            SmalIconPanel.TabIndex = 1;
            SmalIconPanel.DragDrop += SmallIconPanel_DragDrop;
            SmalIconPanel.DragEnter += SmallIconPanel_DragEnter;
            // 
            // AiDoBox
            // 
            AiDoBox.Location = new Point(219, 631);
            AiDoBox.Name = "AiDoBox";
            AiDoBox.Size = new Size(166, 27);
            AiDoBox.TabIndex = 39;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(221, 608);
            label16.Name = "label16";
            label16.Size = new Size(156, 20);
            label16.TabIndex = 40;
            label16.Text = "Желание АИ пикнуть";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(29, 595);
            label17.Name = "label17";
            label17.Size = new Size(94, 20);
            label17.TabIndex = 41;
            label17.Text = "Когда умрет";
            // 
            // ExpireBox
            // 
            ExpireBox.Location = new Point(25, 618);
            ExpireBox.Name = "ExpireBox";
            ExpireBox.Size = new Size(166, 27);
            ExpireBox.TabIndex = 42;
            // 
            // ApplyButton
            // 
            ApplyButton.BackColor = Color.RosyBrown;
            ApplyButton.Location = new Point(291, 673);
            ApplyButton.Name = "ApplyButton";
            ApplyButton.Size = new Size(94, 29);
            ApplyButton.TabIndex = 47;
            ApplyButton.Text = "готово";
            ApplyButton.UseVisualStyleBackColor = false;
            ApplyButton.Click += ApplyButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.BackColor = Color.FromArgb(192, 64, 0);
            SaveButton.Location = new Point(29, 673);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(94, 29);
            SaveButton.TabIndex = 48;
            SaveButton.Text = "сохранить";
            SaveButton.UseVisualStyleBackColor = false;
            SaveButton.Click += SaveButton_Click;
            // 
            // LoadButton
            // 
            LoadButton.BackColor = Color.Green;
            LoadButton.Location = new Point(159, 673);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new Size(94, 29);
            LoadButton.TabIndex = 49;
            LoadButton.Text = "загрузить";
            LoadButton.UseVisualStyleBackColor = false;
            LoadButton.Click += LoadButton_Click;
            // 
            // IdeologyBox
            // 
            IdeologyBox.Location = new Point(219, 441);
            IdeologyBox.Name = "IdeologyBox";
            IdeologyBox.Size = new Size(166, 27);
            IdeologyBox.TabIndex = 50;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(219, 418);
            label18.Name = "label18";
            label18.Size = new Size(76, 20);
            label18.TabIndex = 51;
            label18.Text = "Иделогия";
            // 
            // CharacterCreator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(417, 714);
            Controls.Add(label18);
            Controls.Add(IdeologyBox);
            Controls.Add(LoadButton);
            Controls.Add(SaveButton);
            Controls.Add(ApplyButton);
            Controls.Add(ExpireBox);
            Controls.Add(label17);
            Controls.Add(label16);
            Controls.Add(AiDoBox);
            Controls.Add(SmalIconPanel);
            Controls.Add(label15);
            Controls.Add(TagBox);
            Controls.Add(CharTypesBox);
            Controls.Add(label14);
            Controls.Add(label13);
            Controls.Add(label12);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label7);
            Controls.Add(AdvisorSlot);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(AdvisorCost);
            Controls.Add(SpdBox);
            Controls.Add(SupplyBox);
            Controls.Add(DefBox);
            Controls.Add(AtkBox);
            Controls.Add(SkillBox);
            Controls.Add(label4);
            Controls.Add(IdBox);
            Controls.Add(label8);
            Controls.Add(label3);
            Controls.Add(PercBox);
            Controls.Add(DescBox);
            Controls.Add(label2);
            Controls.Add(NameBox);
            Controls.Add(label1);
            Controls.Add(BigIconPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "CharacterCreator";
            Text = "CharCreator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public TextBox NameBox;
        public RichTextBox PercBox;
        public TextBox DescBox;
        public Panel BigIconPanel;
        public Label label1;
        public Label label2;
        public RichTextBox richTextBox1;
        public Label label3;
        public CheckBox checkBox1;
        public RichTextBox richTextBox2;
        public TextBox IdBox;
        public Label label8;
        public Label label4;
        public TextBox SkillBox;
        public TextBox AtkBox;
        public TextBox DefBox;
        public TextBox SupplyBox;
        public TextBox SpdBox;
        public TextBox AdvisorCost;
        public Label label5;
        public Label label6;
        public ComboBox AdvisorSlot;
        public Label label7;
        public Label label9;
        public Label label10;
        public Label label11;
        public Label label12;
        public Label label13;
        public Label label14;
        public RichTextBox CharTypesBox;
        public TextBox TagBox;
        public Label label15;
        public Panel SmalIconPanel;
        public TextBox AiDoBox;
        public Label label16;
        public Label label17;
        public TextBox ExpireBox;
        public Button ApplyButton;
        public Button SaveButton;
        public Button LoadButton;
        public TextBox IdeologyBox;
        public Label label18;
    }
}