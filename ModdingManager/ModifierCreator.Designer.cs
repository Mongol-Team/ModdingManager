namespace ModdingManager
{
    partial class ModifierCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifierCreator));
            label24 = new Label();
            ModifBox = new RichTextBox();
            SaveConfigButton = new Button();
            ConfigLoadButton = new Button();
            ApplyButton = new Button();
            DescBox = new TextBox();
            NameBox = new TextBox();
            IdBox = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            ImagePanel = new Panel();
            label2 = new Label();
            label6 = new Label();
            label7 = new Label();
            AttackerEffectBox = new RichTextBox();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            RemovalTriggerBox = new RichTextBox();
            label11 = new Label();
            PowerBalanceBox = new RichTextBox();
            TypeBox = new ComboBox();
            VariationBox = new ComboBox();
            EnableBox = new RichTextBox();
            label12 = new Label();
            TriggerBox = new RichTextBox();
            IsTradeBox = new CheckBox();
            DaysBox = new TextBox();
            DecayBox = new TextBox();
            label13 = new Label();
            label14 = new Label();
            MinTrustBox = new TextBox();
            MaxTrustBox = new TextBox();
            label15 = new Label();
            label16 = new Label();
            label17 = new Label();
            ValueBox = new TextBox();
            label18 = new Label();
            TagBox = new TextBox();
            label19 = new Label();
            RelationTrigerBox = new RichTextBox();
            label20 = new Label();
            SuspendLayout();
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(14, 902);
            label24.Name = "label24";
            label24.Size = new Size(0, 20);
            label24.TabIndex = 24;
            // 
            // ModifBox
            // 
            ModifBox.Location = new Point(40, 44);
            ModifBox.Name = "ModifBox";
            ModifBox.Size = new Size(418, 666);
            ModifBox.TabIndex = 25;
            ModifBox.Text = resources.GetString("ModifBox.Text");
            // 
            // SaveConfigButton
            // 
            SaveConfigButton.BackColor = Color.Crimson;
            SaveConfigButton.ForeColor = SystemColors.ControlText;
            SaveConfigButton.Location = new Point(434, 757);
            SaveConfigButton.Name = "SaveConfigButton";
            SaveConfigButton.Size = new Size(315, 29);
            SaveConfigButton.TabIndex = 61;
            SaveConfigButton.Text = "сохранить конфиг";
            SaveConfigButton.UseVisualStyleBackColor = false;
            SaveConfigButton.Click += SaveConfigButton_Click;
            // 
            // ConfigLoadButton
            // 
            ConfigLoadButton.BackColor = Color.OliveDrab;
            ConfigLoadButton.Location = new Point(43, 757);
            ConfigLoadButton.Name = "ConfigLoadButton";
            ConfigLoadButton.Size = new Size(314, 29);
            ConfigLoadButton.TabIndex = 60;
            ConfigLoadButton.Text = "загрузить конфиг";
            ConfigLoadButton.UseVisualStyleBackColor = false;
            ConfigLoadButton.Click += ConfigLoadButton_Click;
            // 
            // ApplyButton
            // 
            ApplyButton.BackColor = Color.RosyBrown;
            ApplyButton.Location = new Point(818, 757);
            ApplyButton.Name = "ApplyButton";
            ApplyButton.Size = new Size(156, 29);
            ApplyButton.TabIndex = 59;
            ApplyButton.Text = "готово";
            ApplyButton.UseVisualStyleBackColor = false;
            ApplyButton.Click += ApplyButton_Click;
            // 
            // DescBox
            // 
            DescBox.Location = new Point(492, 274);
            DescBox.Name = "DescBox";
            DescBox.Size = new Size(218, 27);
            DescBox.TabIndex = 67;
            // 
            // NameBox
            // 
            NameBox.Location = new Point(492, 212);
            NameBox.Name = "NameBox";
            NameBox.Size = new Size(217, 27);
            NameBox.TabIndex = 66;
            // 
            // IdBox
            // 
            IdBox.Location = new Point(492, 145);
            IdBox.Name = "IdBox";
            IdBox.Size = new Size(217, 27);
            IdBox.TabIndex = 65;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(492, 251);
            label5.Name = "label5";
            label5.Size = new Size(138, 20);
            label5.TabIndex = 64;
            label5.Text = "Описание модифа";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(492, 189);
            label4.Name = "label4";
            label4.Size = new Size(98, 20);
            label4.TabIndex = 63;
            label4.Text = "Имя модифа";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(492, 122);
            label3.Name = "label3";
            label3.Size = new Size(104, 20);
            label3.TabIndex = 62;
            label3.Text = "Айди модифа";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(40, 21);
            label1.Name = "label1";
            label1.Size = new Size(411, 20);
            label1.TabIndex = 68;
            label1.Text = "Еффекты модифа(id:значение:скрытопульный? 1 да 0 нет)";
            // 
            // ImagePanel
            // 
            ImagePanel.AllowDrop = true;
            ImagePanel.BackColor = SystemColors.ActiveCaption;
            ImagePanel.ForeColor = SystemColors.ControlLightLight;
            ImagePanel.Location = new Point(520, 58);
            ImagePanel.Name = "ImagePanel";
            ImagePanel.Size = new Size(30, 30);
            ImagePanel.TabIndex = 70;
            ImagePanel.DragDrop += ImagePanel_DragDrop;
            ImagePanel.DragEnter += ImagePanel_DragEnter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(492, 321);
            label2.Name = "label2";
            label2.Size = new Size(149, 20);
            label2.TabIndex = 71;
            label2.Text = "Условие активности";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(649, 21);
            label6.Name = "label6";
            label6.Size = new Size(125, 20);
            label6.TabIndex = 72;
            label6.Text = "тригер удаления";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(465, 428);
            label7.Name = "label7";
            label7.Size = new Size(509, 20);
            label7.TabIndex = 73;
            label7.Text = "еффект оказаный на атакущего(id:значение:скрытопульный? 1 да 0 нет)";
            // 
            // AttackerEffectBox
            // 
            AttackerEffectBox.Location = new Point(490, 451);
            AttackerEffectBox.Name = "AttackerEffectBox";
            AttackerEffectBox.Size = new Size(466, 113);
            AttackerEffectBox.TabIndex = 78;
            AttackerEffectBox.Text = resources.GetString("AttackerEffectBox.Text");
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(477, 21);
            label8.Name = "label8";
            label8.Size = new Size(119, 20);
            label8.TabIndex = 79;
            label8.Text = "Иконка модифа";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(498, 585);
            label9.Name = "label9";
            label9.Size = new Size(235, 20);
            label9.TabIndex = 82;
            label9.Text = "Тип модифа(статик или динами)";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(498, 659);
            label10.Name = "label10";
            label10.Size = new Size(137, 20);
            label10.TabIndex = 83;
            label10.Text = "Вариация модифа";
            // 
            // RemovalTriggerBox
            // 
            RemovalTriggerBox.Location = new Point(610, 44);
            RemovalTriggerBox.Name = "RemovalTriggerBox";
            RemovalTriggerBox.Size = new Size(206, 64);
            RemovalTriggerBox.TabIndex = 84;
            RemovalTriggerBox.Text = "";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(748, 119);
            label11.Name = "label11";
            label11.Size = new Size(226, 40);
            label11.TabIndex = 85;
            label11.Text = "изменение баланса сил\r\n(ид:значение:скрыт? 1 да 0 нет)\r\n";
            // 
            // PowerBalanceBox
            // 
            PowerBalanceBox.Location = new Point(750, 162);
            PowerBalanceBox.Name = "PowerBalanceBox";
            PowerBalanceBox.Size = new Size(206, 47);
            PowerBalanceBox.TabIndex = 86;
            PowerBalanceBox.Text = "power_balance_weekly\npower_balance_daily";
            // 
            // TypeBox
            // 
            TypeBox.FormattingEnabled = true;
            TypeBox.Items.AddRange(new object[] { "dynamic", "static", "opinion" });
            TypeBox.Location = new Point(498, 608);
            TypeBox.Name = "TypeBox";
            TypeBox.Size = new Size(235, 28);
            TypeBox.TabIndex = 87;
            // 
            // VariationBox
            // 
            VariationBox.FormattingEnabled = true;
            VariationBox.Items.AddRange(new object[] { "power_balance", "default", "relation", "province" });
            VariationBox.Location = new Point(498, 682);
            VariationBox.Name = "VariationBox";
            VariationBox.Size = new Size(235, 28);
            VariationBox.TabIndex = 88;
            // 
            // EnableBox
            // 
            EnableBox.Location = new Point(490, 344);
            EnableBox.Name = "EnableBox";
            EnableBox.Size = new Size(206, 64);
            EnableBox.TabIndex = 89;
            EnableBox.Text = "";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(784, 219);
            label12.Name = "label12";
            label12.Size = new Size(137, 20);
            label12.TabIndex = 90;
            label12.Text = "тригер активность";
            // 
            // TriggerBox
            // 
            TriggerBox.Location = new Point(750, 242);
            TriggerBox.Name = "TriggerBox";
            TriggerBox.Size = new Size(206, 64);
            TriggerBox.TabIndex = 91;
            TriggerBox.Text = "";
            // 
            // IsTradeBox
            // 
            IsTradeBox.AutoSize = true;
            IsTradeBox.Location = new Point(748, 331);
            IsTradeBox.Name = "IsTradeBox";
            IsTradeBox.Size = new Size(98, 24);
            IsTradeBox.TabIndex = 92;
            IsTradeBox.Text = "торговый";
            IsTradeBox.UseVisualStyleBackColor = true;
            // 
            // DaysBox
            // 
            DaysBox.Location = new Point(800, 381);
            DaysBox.Name = "DaysBox";
            DaysBox.Size = new Size(48, 27);
            DaysBox.TabIndex = 93;
            // 
            // DecayBox
            // 
            DecayBox.Location = new Point(864, 381);
            DecayBox.Name = "DecayBox";
            DecayBox.Size = new Size(48, 27);
            DecayBox.TabIndex = 94;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(800, 358);
            label13.Name = "label13";
            label13.Size = new Size(35, 20);
            label13.TabIndex = 95;
            label13.Text = "дни";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(864, 358);
            label14.Name = "label14";
            label14.Size = new Size(49, 20);
            label14.TabIndex = 96;
            label14.Text = "декай";
            // 
            // MinTrustBox
            // 
            MinTrustBox.Location = new Point(835, 81);
            MinTrustBox.Name = "MinTrustBox";
            MinTrustBox.Size = new Size(48, 27);
            MinTrustBox.TabIndex = 97;
            // 
            // MaxTrustBox
            // 
            MaxTrustBox.Location = new Point(908, 81);
            MaxTrustBox.Name = "MaxTrustBox";
            MaxTrustBox.Size = new Size(48, 27);
            MaxTrustBox.TabIndex = 98;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(835, 58);
            label15.Name = "label15";
            label15.Size = new Size(38, 20);
            label15.TabIndex = 99;
            label15.Text = "мин";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(914, 58);
            label16.Name = "label16";
            label16.Size = new Size(42, 20);
            label16.TabIndex = 100;
            label16.Text = "макс";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(822, 38);
            label17.Name = "label17";
            label17.Size = new Size(138, 20);
            label17.TabIndex = 101;
            label17.Text = "разброс значений";
            // 
            // ValueBox
            // 
            ValueBox.Location = new Point(736, 381);
            ValueBox.Name = "ValueBox";
            ValueBox.Size = new Size(48, 27);
            ValueBox.TabIndex = 102;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(743, 358);
            label18.Name = "label18";
            label18.Size = new Size(41, 20);
            label18.TabIndex = 103;
            label18.Text = "знач";
            // 
            // TagBox
            // 
            TagBox.Location = new Point(757, 609);
            TagBox.Name = "TagBox";
            TagBox.Size = new Size(78, 27);
            TagBox.TabIndex = 104;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(757, 585);
            label19.Name = "label19";
            label19.Size = new Size(159, 20);
            label19.TabIndex = 105;
            label19.Text = "Пренадлежность тегу";
            // 
            // RelationTrigerBox
            // 
            RelationTrigerBox.Location = new Point(750, 659);
            RelationTrigerBox.Name = "RelationTrigerBox";
            RelationTrigerBox.Size = new Size(206, 64);
            RelationTrigerBox.TabIndex = 106;
            RelationTrigerBox.Text = "";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(741, 636);
            label20.Name = "label20";
            label20.Size = new Size(227, 20);
            label20.TabIndex = 107;
            label20.Text = "относительный тригер модифа";
            // 
            // ModifierCreator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(986, 809);
            Controls.Add(label20);
            Controls.Add(RelationTrigerBox);
            Controls.Add(label19);
            Controls.Add(TagBox);
            Controls.Add(label18);
            Controls.Add(ValueBox);
            Controls.Add(label17);
            Controls.Add(label16);
            Controls.Add(label15);
            Controls.Add(MaxTrustBox);
            Controls.Add(MinTrustBox);
            Controls.Add(label14);
            Controls.Add(label13);
            Controls.Add(DecayBox);
            Controls.Add(DaysBox);
            Controls.Add(IsTradeBox);
            Controls.Add(TriggerBox);
            Controls.Add(label12);
            Controls.Add(EnableBox);
            Controls.Add(VariationBox);
            Controls.Add(TypeBox);
            Controls.Add(PowerBalanceBox);
            Controls.Add(label11);
            Controls.Add(RemovalTriggerBox);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(AttackerEffectBox);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label2);
            Controls.Add(ImagePanel);
            Controls.Add(label1);
            Controls.Add(DescBox);
            Controls.Add(NameBox);
            Controls.Add(IdBox);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(SaveConfigButton);
            Controls.Add(ConfigLoadButton);
            Controls.Add(ApplyButton);
            Controls.Add(ModifBox);
            Controls.Add(label24);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(2000000, 2000000);
            Name = "ModifierCreator";
            Text = "Модифер креатор";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label24;
        public RichTextBox ModifBox;
        public Button SaveConfigButton;
        public Button ConfigLoadButton;
        public Button ApplyButton;
        public TextBox DescBox;
        public TextBox NameBox;
        public TextBox IdBox;
        public Label label5;
        public Label label4;
        public Label label3;
        public Label label1;
        public Panel ImagePanel;
        public Label label2;
        public Label label6;
        public Label label7;
        public TextBox Box;
        public RichTextBox AttackerEffectBox;
        public Label label8;
        public Label label9;
        public Label label10;
        public RichTextBox RemovalTriggerBox;
        public Label label11;
        public RichTextBox PowerBalanceBox;
        public ComboBox TypeBox;
        public ComboBox VariationBox;
        public RichTextBox EnableBox;
        public Label label12;
        public RichTextBox TriggerBox;
        public CheckBox IsTradeBox;
        public TextBox DaysBox;
        public TextBox DecayBox;
        public Label label13;
        public Label label14;
        public TextBox MinTrustBox;
        public TextBox MaxTrustBox;
        public Label label15;
        public Label label16;
        public Label label17;
        public TextBox ValueBox;
        public Label label18;
        public TextBox TagBox;
        public Label label19;
        public RichTextBox RelationTrigerBox;
        public Label label20;
    }
}