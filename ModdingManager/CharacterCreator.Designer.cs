namespace ModdingManager
{
    partial class CharacterCreator
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
            panel1 = new Panel();
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            textBox2 = new TextBox();
            richTextBox1 = new RichTextBox();
            label3 = new Label();
            label4 = new Label();
            checkBox1 = new CheckBox();
            panel2 = new Panel();
            label5 = new Label();
            richTextBox2 = new RichTextBox();
            textBox3 = new TextBox();
            label6 = new Label();
            textBox4 = new TextBox();
            label7 = new Label();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Location = new Point(30, 23);
            panel1.Name = "panel1";
            panel1.Size = new Size(156, 210);
            panel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(249, 23);
            label1.Name = "label1";
            label1.Size = new Size(75, 20);
            label1.TabIndex = 1;
            label1.Text = "Имя типа";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(245, 46);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(166, 27);
            textBox1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(245, 87);
            label2.Name = "label2";
            label2.Size = new Size(79, 20);
            label2.TabIndex = 3;
            label2.Text = "Описание";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(245, 110);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(166, 27);
            textBox2.TabIndex = 4;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(245, 178);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(166, 55);
            richTextBox1.TabIndex = 5;
            richTextBox1.Text = "";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(245, 155);
            label3.Name = "label3";
            label3.Size = new Size(53, 20);
            label3.TabIndex = 6;
            label3.Text = "Перки";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(149, 274);
            label4.Name = "label4";
            label4.Size = new Size(244, 20);
            label4.TabIndex = 7;
            label4.Text = "Правитель или леджер советник?";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(99, 277);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(18, 17);
            checkBox1.TabIndex = 8;
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveCaption;
            panel2.Location = new Point(52, 326);
            panel2.Name = "panel2";
            panel2.Size = new Size(65, 67);
            panel2.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(227, 446);
            label5.Name = "label5";
            label5.Size = new Size(53, 20);
            label5.TabIndex = 14;
            label5.Text = "Перки";
            // 
            // richTextBox2
            // 
            richTextBox2.Location = new Point(227, 469);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(166, 55);
            richTextBox2.TabIndex = 13;
            richTextBox2.Text = "";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(227, 401);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(166, 27);
            textBox3.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(227, 378);
            label6.Name = "label6";
            label6.Size = new Size(79, 20);
            label6.TabIndex = 11;
            label6.Text = "Описание";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(227, 337);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(166, 27);
            textBox4.TabIndex = 10;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(231, 314);
            label7.Name = "label7";
            label7.Size = new Size(75, 20);
            label7.TabIndex = 9;
            label7.Text = "Имя типа";
            // 
            // CharacterCreator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(466, 568);
            Controls.Add(label5);
            Controls.Add(richTextBox2);
            Controls.Add(textBox3);
            Controls.Add(label6);
            Controls.Add(textBox4);
            Controls.Add(label7);
            Controls.Add(panel2);
            Controls.Add(checkBox1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(richTextBox1);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(panel1);
            Name = "CharacterCreator";
            Text = "CharCreator";
            Load += this.CharacterCreator_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private TextBox textBox2;
        private RichTextBox richTextBox1;
        private Label label3;
        private Label label4;
        private CheckBox checkBox1;
        private Panel panel2;
        private Label label5;
        private RichTextBox richTextBox2;
        private TextBox textBox3;
        private Label label6;
        private TextBox textBox4;
        private Label label7;
    }
}