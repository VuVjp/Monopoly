namespace Monopoly
{
    partial class GameConfig
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
            btnCreate = new Button();
            btnJoin = new Button();
            lsbGameContainer = new ListBox();
            btnReturn = new Button();
            btnDelete = new Button();
            lblTitle = new Label();
            SuspendLayout();
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("Showcard Gothic", 24F);
            btnCreate.Location = new Point(185, 718);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(560, 86);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "Create New Game";
            btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnJoin
            // 
            btnJoin.Font = new Font("Showcard Gothic", 24F);
            btnJoin.Location = new Point(766, 718);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(560, 86);
            btnJoin.TabIndex = 3;
            btnJoin.Text = "Join Game";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += btnJoin_Click;
            // 
            // lsbGameContainer
            // 
            lsbGameContainer.FormattingEnabled = true;
            lsbGameContainer.Location = new Point(185, 134);
            lsbGameContainer.Name = "lsbGameContainer";
            lsbGameContainer.Size = new Size(1141, 554);
            lsbGameContainer.TabIndex = 4;
            // 
            // btnReturn
            // 
            btnReturn.Font = new Font("Showcard Gothic", 24F);
            btnReturn.Location = new Point(766, 810);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(560, 86);
            btnReturn.TabIndex = 5;
            btnReturn.Text = "Return";
            btnReturn.UseVisualStyleBackColor = true;
            btnReturn.Click += btnReturn_Click;
            // 
            // btnDelete
            // 
            btnDelete.Font = new Font("Showcard Gothic", 24F);
            btnDelete.Location = new Point(185, 810);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(560, 86);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "Delete Game";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Showcard Gothic", 50F);
            lblTitle.ForeColor = Color.White;
            lblTitle.ImageAlign = ContentAlignment.MiddleRight;
            lblTitle.Location = new Point(-1, 7);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1518, 124);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "Singleplayer";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // GameConfig
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 46);
            ClientSize = new Size(1517, 917);
            Controls.Add(lblTitle);
            Controls.Add(btnDelete);
            Controls.Add(btnReturn);
            Controls.Add(lsbGameContainer);
            Controls.Add(btnJoin);
            Controls.Add(btnCreate);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "GameConfig";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GameConfig";
            FormClosing += GameConfig_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private Button btnCreate;
        private Button btnJoin;
        private ListBox lsbGameContainer;
        private Button btnReturn;
        private Button btnDelete;
        private Label lblTitle;
    }
}