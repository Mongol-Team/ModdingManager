namespace ModdingManager
{
    partial class FlagCreator
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
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            label3 = new Label();
            panel2 = new Panel();
            panel3 = new Panel();
            panel4 = new Panel();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AllowDrop = true;
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Location = new Point(27, 45);
            panel1.Name = "panel1";
            panel1.Size = new Size(82, 52);
            panel1.TabIndex = 0;
            panel1.DragDrop += panel1_DragDrop;
            panel1.DragEnter += panel1_DragEnter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 9);
            label1.Name = "label1";
            label1.Size = new Size(162, 20);
            label1.TabIndex = 1;
            label1.Text = "перетянуть сюда флаг";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(233, 21);
            label2.Name = "label2";
            label2.Size = new Size(136, 20);
            label2.TabIndex = 2;
            label2.Text = "путь для создания";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(244, 60);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(125, 27);
            textBox1.TabIndex = 3;
            textBox1.KeyDown += textBox1_KeyDown;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(244, 125);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(125, 27);
            textBox2.TabIndex = 4;
            textBox2.KeyDown += textBox2_KeyDown;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(286, 102);
            label3.Name = "label3";
            label3.Size = new Size(83, 20);
            label3.TabIndex = 5;
            label3.Text = "тег страны";
            // 
            // panel2
            // 
            panel2.AllowDrop = true;
            panel2.BackColor = SystemColors.ActiveCaption;
            panel2.Location = new Point(27, 125);
            panel2.Name = "panel2";
            panel2.Size = new Size(82, 52);
            panel2.TabIndex = 1;
            panel2.DragDrop += panel1_DragDrop;
            panel2.DragEnter += panel1_DragEnter;
            // 
            // panel3
            // 
            panel3.AllowDrop = true;
            panel3.BackColor = SystemColors.ActiveCaption;
            panel3.Location = new Point(27, 198);
            panel3.Name = "panel3";
            panel3.Size = new Size(82, 52);
            panel3.TabIndex = 6;
            panel3.DragDrop += panel1_DragDrop;
            panel3.DragEnter += panel1_DragEnter;
            // 
            // panel4
            // 
            panel4.AllowDrop = true;
            panel4.BackColor = SystemColors.ActiveCaption;
            panel4.Location = new Point(27, 274);
            panel4.Name = "panel4";
            panel4.Size = new Size(82, 52);
            panel4.TabIndex = 1;
            panel4.DragDrop += panel1_DragDrop;
            panel4.DragEnter += panel1_DragEnter;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(115, 63);
            label4.Name = "label4";
            label4.Size = new Size(66, 20);
            label4.TabIndex = 7;
            label4.Text = "нейтрал";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(115, 146);
            label5.Name = "label5";
            label5.Size = new Size(66, 20);
            label5.TabIndex = 8;
            label5.Text = "фашизм";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(115, 219);
            label6.Name = "label6";
            label6.Size = new Size(90, 20);
            label6.TabIndex = 9;
            label6.Text = "коммунизм";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(115, 294);
            label7.Name = "label7";
            label7.Size = new Size(86, 20);
            label7.TabIndex = 10;
            label7.Text = "демократы";
            // 
            // FlagCreator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(378, 394);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(panel1);
            Name = "FlagCreator";
            Text = "FlagCreator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label label3;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
    }
}