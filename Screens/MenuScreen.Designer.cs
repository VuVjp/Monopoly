namespace Monopoly.Screens
{
    partial class MenuScreen
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            button1 = new Button();
            btnQuit = new Button();
            btnAbout = new Button();
            btnOnline = new Button();
            btnSingle = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Red;
            lblTitle.Font = new Font("Showcard Gothic", 100F);
            lblTitle.ForeColor = Color.WhiteSmoke;
            lblTitle.Location = new Point(316, 281);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1192, 249);
            lblTitle.TabIndex = 14;
            lblTitle.Text = "Monopoly";
            // 
            // button1
            // 
            button1.Font = new Font("Showcard Gothic", 24F);
            button1.Location = new Point(433, 767);
            button1.Name = "button1";
            button1.Size = new Size(952, 86);
            button1.TabIndex = 13;
            button1.Text = "How To Play";
            button1.UseVisualStyleBackColor = true;
            // 
            // btnQuit
            // 
            btnQuit.Font = new Font("Showcard Gothic", 24F);
            btnQuit.Location = new Point(929, 901);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(456, 86);
            btnQuit.TabIndex = 12;
            btnQuit.Text = "Quit Game";
            btnQuit.UseVisualStyleBackColor = true;
            // 
            // btnAbout
            // 
            btnAbout.Font = new Font("Showcard Gothic", 24F);
            btnAbout.Location = new Point(433, 901);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(456, 86);
            btnAbout.TabIndex = 11;
            btnAbout.Text = "About Us";
            btnAbout.UseVisualStyleBackColor = true;
            // 
            // btnOnline
            // 
            btnOnline.Font = new Font("Showcard Gothic", 24F);
            btnOnline.Location = new Point(433, 666);
            btnOnline.Name = "btnOnline";
            btnOnline.Size = new Size(952, 86);
            btnOnline.TabIndex = 10;
            btnOnline.Text = "Multiplayer";
            btnOnline.UseVisualStyleBackColor = true;
            // 
            // btnSingle
            // 
            btnSingle.Font = new Font("Showcard Gothic", 24F);
            btnSingle.Location = new Point(433, 565);
            btnSingle.Name = "btnSingle";
            btnSingle.Size = new Size(952, 86);
            btnSingle.TabIndex = 9;
            btnSingle.Text = "Singleplayer";
            btnSingle.UseVisualStyleBackColor = true;
            // 
            // MenuScreen
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(233, 227, 208);
            Controls.Add(lblTitle);
            Controls.Add(button1);
            Controls.Add(btnQuit);
            Controls.Add(btnAbout);
            Controls.Add(btnOnline);
            Controls.Add(btnSingle);
            Name = "MenuScreen";
            Size = new Size(1824, 1268);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Button button1;
        private Button btnQuit;
        private Button btnAbout;
        private Button btnOnline;
        private Button btnSingle;
    }
}
