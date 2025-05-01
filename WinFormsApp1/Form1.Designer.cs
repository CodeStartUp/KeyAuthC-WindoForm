namespace WinFormsApp1
{
    partial class Form1
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
            usern = new TextBox();
            pass = new TextBox();
            button1 = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // usern
            // 
            usern.Location = new Point(291, 206);
            usern.Name = "usern";
            usern.Size = new Size(212, 27);
            usern.TabIndex = 0;
            usern.TextChanged += textBox1_TextChanged;
            // 
            // pass
            // 
            pass.Location = new Point(291, 256);
            pass.Name = "pass";
            pass.Size = new Size(212, 27);
            pass.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(339, 323);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 2;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 399);
            label1.Name = "label1";
            label1.Size = new Size(50, 20);
            label1.TabIndex = 3;
            label1.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(pass);
            Controls.Add(usern);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox usern;
        private TextBox pass;
        private Button button1;
        private Label label1;
    }
}
