namespace ModdingManager
{
    partial class IdeaCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IdeaCreator));
            ModifBox = new RichTextBox();
            label1 = new Label();
            ImagePanel = new Panel();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            IdBox = new TextBox();
            NameBox = new TextBox();
            DescBox = new TextBox();
            TagBox = new TextBox();
            label6 = new Label();
            AvaibleBox = new RichTextBox();
            RemovalCostBox = new TextBox();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            AvaibleCivBox = new RichTextBox();
            label10 = new Label();
            OnAddBox = new RichTextBox();
            ApplyButton = new Button();
            SuspendLayout();
            // 
            // ModifBox
            // 
            ModifBox.Location = new Point(26, 55);
            ModifBox.Name = "ModifBox";
            ModifBox.Size = new Size(418, 521);
            ModifBox.TabIndex = 0;
            ModifBox.Text = resources.GetString("ModifBox.Text");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 21);
            label1.Name = "label1";
            label1.Size = new Size(155, 20);
            label1.TabIndex = 1;
            label1.Text = "Модификаторы идеи";
            // 
            // ImagePanel
            // 
            ImagePanel.AllowDrop = true;
            ImagePanel.BackColor = SystemColors.ActiveCaption;
            ImagePanel.ForeColor = SystemColors.ControlLightLight;
            ImagePanel.Location = new Point(474, 55);
            ImagePanel.Name = "ImagePanel";
            ImagePanel.Size = new Size(64, 64);
            ImagePanel.TabIndex = 2;
            ImagePanel.DragDrop += ImagePanel_DragDrop;
            ImagePanel.DragEnter += ImagePanel_DragEnter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(478, 21);
            label2.Name = "label2";
            label2.Size = new Size(60, 20);
            label2.TabIndex = 3;
            label2.Text = "Иконка";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(474, 129);
            label3.Name = "label3";
            label3.Size = new Size(83, 20);
            label3.TabIndex = 4;
            label3.Text = "Айди идеи";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(474, 187);
            label4.Name = "label4";
            label4.Size = new Size(77, 20);
            label4.TabIndex = 5;
            label4.Text = "Имя идеи";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(474, 244);
            label5.Name = "label5";
            label5.Size = new Size(117, 20);
            label5.TabIndex = 6;
            label5.Text = "Описание идеи";
            // 
            // IdBox
            // 
            IdBox.Location = new Point(474, 152);
            IdBox.Name = "IdBox";
            IdBox.Size = new Size(217, 27);
            IdBox.TabIndex = 7;
            // 
            // NameBox
            // 
            NameBox.Location = new Point(474, 210);
            NameBox.Name = "NameBox";
            NameBox.Size = new Size(217, 27);
            NameBox.TabIndex = 8;
            // 
            // DescBox
            // 
            DescBox.Location = new Point(474, 267);
            DescBox.Name = "DescBox";
            DescBox.Size = new Size(218, 27);
            DescBox.TabIndex = 9;
            // 
            // TagBox
            // 
            TagBox.Location = new Point(474, 326);
            TagBox.Name = "TagBox";
            TagBox.Size = new Size(218, 27);
            TagBox.TabIndex = 10;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(474, 303);
            label6.Name = "label6";
            label6.Size = new Size(160, 20);
            label6.TabIndex = 11;
            label6.Text = "Принадлежность тегу";
            // 
            // AvaibleBox
            // 
            AvaibleBox.Location = new Point(478, 435);
            AvaibleBox.Name = "AvaibleBox";
            AvaibleBox.Size = new Size(213, 58);
            AvaibleBox.TabIndex = 12;
            AvaibleBox.Text = "";
            // 
            // RemovalCostBox
            // 
            RemovalCostBox.Location = new Point(474, 382);
            RemovalCostBox.Name = "RemovalCostBox";
            RemovalCostBox.Size = new Size(217, 27);
            RemovalCostBox.TabIndex = 13;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(475, 359);
            label7.Name = "label7";
            label7.Size = new Size(166, 20);
            label7.TabIndex = 14;
            label7.Text = "Цена удаления(почки)";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(478, 412);
            label8.Name = "label8";
            label8.Size = new Size(93, 20);
            label8.TabIndex = 15;
            label8.Text = "разрешалка";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(450, 496);
            label9.Name = "label9";
            label9.Size = new Size(241, 20);
            label9.TabIndex = 17;
            label9.Text = "разрешалка во время гражданки";
            // 
            // AvaibleCivBox
            // 
            AvaibleCivBox.Location = new Point(478, 518);
            AvaibleCivBox.Name = "AvaibleCivBox";
            AvaibleCivBox.Size = new Size(213, 58);
            AvaibleCivBox.TabIndex = 16;
            AvaibleCivBox.Text = "";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(567, 21);
            label10.Name = "label10";
            label10.Size = new Size(125, 20);
            label10.TabIndex = 18;
            label10.Text = "при добавлении";
            // 
            // OnAddBox
            // 
            OnAddBox.Location = new Point(554, 61);
            OnAddBox.Name = "OnAddBox";
            OnAddBox.Size = new Size(137, 58);
            OnAddBox.TabIndex = 19;
            OnAddBox.Text = "";
            // 
            // ApplyButton
            // 
            ApplyButton.BackColor = Color.RosyBrown;
            ApplyButton.Location = new Point(598, 602);
            ApplyButton.Name = "ApplyButton";
            ApplyButton.Size = new Size(94, 29);
            ApplyButton.TabIndex = 47;
            ApplyButton.Text = "готово";
            ApplyButton.UseVisualStyleBackColor = false;
            ApplyButton.Click += ApplyButton_Click;
            // 
            // IdeaCreator
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(716, 643);
            Controls.Add(ApplyButton);
            Controls.Add(OnAddBox);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(AvaibleCivBox);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(RemovalCostBox);
            Controls.Add(AvaibleBox);
            Controls.Add(label6);
            Controls.Add(TagBox);
            Controls.Add(DescBox);
            Controls.Add(NameBox);
            Controls.Add(IdBox);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(ImagePanel);
            Controls.Add(label1);
            Controls.Add(ModifBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "IdeaCreator";
            Text = "IdeaCreator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public RichTextBox ModifBox;
        public Label label1;
        public Panel ImagePanel;
        public Label label2;
        public Label label3;
        public Label label4;
        public Label label5;
        public TextBox IdBox;
        public TextBox NameBox;
        public TextBox DescBox;
        public TextBox TagBox;
        public Label label6;
        public RichTextBox AvaibleBox;
        public TextBox RemovalCostBox;
        public Label label7;
        public Label label8;
        public Label label9;
        public RichTextBox AvaibleCivBox;
        public Label label10;
        public RichTextBox OnAddBox;
        public Button ApplyButton;
    }
}