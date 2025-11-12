namespace Monopoly.Screens
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            btnDelete = new Button();
            btnReturn = new Button();
            lsbGameContainer = new ListBox();
            btnJoin = new Button();
            btnCreate = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Showcard Gothic", 50F);
            lblTitle.ForeColor = Color.White;
            lblTitle.ImageAlign = ContentAlignment.MiddleRight;
            lblTitle.Location = new Point(0, 20);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1253, 74);
            lblTitle.TabIndex = 14;
            lblTitle.Text = "Singleplayer";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnDelete
            // 
            btnDelete.Font = new Font("Showcard Gothic", 24F);
            btnDelete.Location = new Point(227, 629);
            btnDelete.Margin = new Padding(2);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(392, 52);
            btnDelete.TabIndex = 13;
            btnDelete.Text = "Delete Game";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnReturn
            // 
            btnReturn.Font = new Font("Showcard Gothic", 24F);
            btnReturn.Location = new Point(634, 629);
            btnReturn.Margin = new Padding(2);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(392, 52);
            btnReturn.TabIndex = 12;
            btnReturn.Text = "Return";
            btnReturn.UseVisualStyleBackColor = true;
            btnReturn.Click += btnReturn_Click;
            // 
            // lsbGameContainer
            // 
            lsbGameContainer.Font = new Font("Segoe UI", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lsbGameContainer.FormattingEnabled = true;
            lsbGameContainer.Location = new Point(227, 114);
            lsbGameContainer.Margin = new Padding(2);
            lsbGameContainer.Name = "lsbGameContainer";
            lsbGameContainer.Size = new Size(800, 414);
            lsbGameContainer.TabIndex = 11;
            // 
            // btnJoin
            // 
            btnJoin.Font = new Font("Showcard Gothic", 24F);
            btnJoin.Location = new Point(634, 574);
            btnJoin.Margin = new Padding(2);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(392, 52);
            btnJoin.TabIndex = 10;
            btnJoin.Text = "Join Game";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += btnJoin_Click;
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("Showcard Gothic", 24F);
            btnCreate.Location = new Point(227, 574);
            btnCreate.Margin = new Padding(2);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(392, 52);
            btnCreate.TabIndex = 9;
            btnCreate.Text = "Create New Game";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // GameConfig
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 46);
            Controls.Add(lblTitle);
            Controls.Add(btnDelete);
            Controls.Add(btnReturn);
            Controls.Add(lsbGameContainer);
            Controls.Add(btnJoin);
            Controls.Add(btnCreate);
            Margin = new Padding(2);
            Name = "GameConfig";
            Size = new Size(1255, 728);
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Button btnDelete;
        private Button btnReturn;
        private ListBox lsbGameContainer;
        private Button btnJoin;
        private Button btnCreate;
    }
}
