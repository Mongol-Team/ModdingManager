namespace ModdingManager
{
    partial class TechnologyGenerator
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
            dataGridView1 = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            textBox4 = new TextBox();
            textBox5 = new TextBox();
            textBox6 = new TextBox();
            label9 = new Label();
            checkedListBox1 = new CheckedListBox();
            richTextBox1 = new RichTextBox();
            richTextBox2 = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(577, 37);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(582, 406);
            dataGridView1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(799, 9);
            label1.Name = "label1";
            label1.Size = new Size(150, 20);
            label1.TabIndex = 1;
            label1.Text = "текущие технологии";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(121, 20);
            label2.TabIndex = 2;
            label2.Text = "имя технологии";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 67);
            label3.Name = "label3";
            label3.Size = new Size(183, 20);
            label3.TabIndex = 3;
            label3.Text = "стоимость исследования";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 121);
            label4.Name = "label4";
            label4.Size = new Size(315, 20);
            label4.TabIndex = 4;
            label4.Text = "ведет к какой то технологии\\коеф стоимоти";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 174);
            label5.Name = "label5";
            label5.Size = new Size(50, 20);
            label5.TabIndex = 5;
            label5.Text = "папка";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 225);
            label6.Name = "label6";
            label6.Size = new Size(185, 20);
            label6.TabIndex = 6;
            label6.Text = "категории через запятую";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 272);
            label7.Name = "label7";
            label7.Size = new Size(91, 20);
            label7.TabIndex = 7;
            label7.Text = "желание аи";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(361, 216);
            label8.Name = "label8";
            label8.Size = new Size(188, 20);
            label8.TabIndex = 8;
            label8.Text = "модификатор технологии";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 37);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(125, 27);
            textBox1.TabIndex = 9;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(12, 91);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(125, 27);
            textBox2.TabIndex = 10;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 144);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(125, 27);
            textBox3.TabIndex = 11;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(12, 197);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(125, 27);
            textBox4.TabIndex = 12;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(12, 248);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(125, 27);
            textBox5.TabIndex = 13;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(12, 301);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(125, 27);
            textBox6.TabIndex = 14;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(12, 388);
            label9.Name = "label9";
            label9.Size = new Size(374, 20);
            label9.TabIndex = 17;
            label9.Text = "имя категорри для добавлния бонуса\\модификатор";
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Items.AddRange(new object[] { "category_tanks", "category_light_infantry", "category_front_line", "category_all_infantry", "category_support_battalions", "category_militia", "category_all_armor", "category_army", "category_line_artillery", "category_artillery", "category_fighter", "category_heavy_fighter", "category_cas", "category_special_forces", "category_nav_bomber", "category_tac_bomber", "category_strat_bomber", "category_scout_plane", "category_recon", "category_mountaineers", "category_paratroopers", "category_marines", "category_anti_air", "category_amphibious_tanks", "category_support_artillery", "category_cavalry", "category_bus", "category_vehicle_infantry", "category_special_forces_leg_infantry", "category_mobile_and_mobile_combat_sup", "category_infantry_and_bicycle", "category_self_propelled_artillery", "category_self_propelled_anti_air", "category_tank_destroyers", "category_helicopter_support_companies" });
            checkedListBox1.Location = new Point(12, 411);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(238, 224);
            checkedListBox1.TabIndex = 18;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(266, 411);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(283, 224);
            richTextBox1.TabIndex = 19;
            richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            richTextBox2.Location = new Point(378, 252);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(171, 133);
            richTextBox2.TabIndex = 20;
            richTextBox2.Text = "";
            // 
            // TechnologyGenerator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1212, 649);
            Controls.Add(richTextBox2);
            Controls.Add(richTextBox1);
            Controls.Add(checkedListBox1);
            Controls.Add(label9);
            Controls.Add(textBox6);
            Controls.Add(textBox5);
            Controls.Add(textBox4);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dataGridView1);
            Name = "TechnologyGenerator";
            Text = "TechnologyGenerator";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private TextBox textBox5;
        private TextBox textBox6;
        private Label label9;
        private CheckedListBox checkedListBox1;
        private RichTextBox richTextBox1;
        private RichTextBox richTextBox2;
    }
}