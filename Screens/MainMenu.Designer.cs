namespace Monopoly.Screens
{
    partial class MainMenu
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
            btnHowToPlay = new Button();
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
            lblTitle.Font = new Font("Showcard Gothic", 120F);
            lblTitle.ForeColor = Color.WhiteSmoke;
            lblTitle.Location = new Point(142, 97);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(955, 199);
            lblTitle.TabIndex = 14;
            lblTitle.Text = "Monopoly";
            // 
            // btnHowToPlay
            // 
            btnHowToPlay.Font = new Font("Showcard Gothic", 24F);
            btnHowToPlay.Location = new Point(285, 463);
            btnHowToPlay.Margin = new Padding(2);
            btnHowToPlay.Name = "btnHowToPlay";
            btnHowToPlay.Size = new Size(666, 52);
            btnHowToPlay.TabIndex = 13;
            btnHowToPlay.Text = "How To Play";
            btnHowToPlay.UseVisualStyleBackColor = true;
            // 
            // btnQuit
            // 
            btnQuit.Font = new Font("Showcard Gothic", 24F);
            btnQuit.Location = new Point(632, 544);
            btnQuit.Margin = new Padding(2);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(319, 52);
            btnQuit.TabIndex = 12;
            btnQuit.Text = "Quit Game";
            btnQuit.UseVisualStyleBackColor = true;
            btnQuit.Click += btnQuit_Click;
            // 
            // btnAbout
            // 
            btnAbout.Font = new Font("Showcard Gothic", 24F);
            btnAbout.Location = new Point(285, 544);
            btnAbout.Margin = new Padding(2);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(319, 52);
            btnAbout.TabIndex = 11;
            btnAbout.Text = "About Us";
            btnAbout.UseVisualStyleBackColor = true;
            // 
            // btnOnline
            // 
            btnOnline.Font = new Font("Showcard Gothic", 24F);
            btnOnline.Location = new Point(285, 403);
            btnOnline.Margin = new Padding(2);
            btnOnline.Name = "btnOnline";
            btnOnline.Size = new Size(666, 52);
            btnOnline.TabIndex = 10;
            btnOnline.Text = "Multiplayer";
            btnOnline.UseVisualStyleBackColor = true;
            // 
            // btnSingle
            // 
            btnSingle.Font = new Font("Showcard Gothic", 24F);
            btnSingle.Location = new Point(285, 342);
            btnSingle.Margin = new Padding(2);
            btnSingle.Name = "btnSingle";
            btnSingle.Size = new Size(666, 52);
            btnSingle.TabIndex = 9;
            btnSingle.Text = "Singleplayer";
            btnSingle.UseVisualStyleBackColor = true;
            btnSingle.Click += btnSingle_Click;
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(233, 227, 208);
            Controls.Add(lblTitle);
            Controls.Add(btnHowToPlay);
            Controls.Add(btnQuit);
            Controls.Add(btnAbout);
            Controls.Add(btnOnline);
            Controls.Add(btnSingle);
            Margin = new Padding(2);
            Name = "MainMenu";
            Size = new Size(1255, 728);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Button btnHowToPlay;
        private Button btnQuit;
        private Button btnAbout;
        private Button btnOnline;
        private Button btnSingle;
    }
}
