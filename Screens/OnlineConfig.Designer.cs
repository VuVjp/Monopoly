namespace Monopoly.Screens
{
    partial class OnlineConfig
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
            btnCreate = new Button();
            btReturn = new Button();
            lblTitle = new Label();
            lblPlayerCount = new Label();
            lblCounter = new Label();
            btnMinus = new Button();
            btnPlus = new Button();
            SuspendLayout();
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("Showcard Gothic", 24F);
            btnCreate.Location = new Point(629, 541);
            btnCreate.Margin = new Padding(2);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(392, 52);
            btnCreate.TabIndex = 12;
            btnCreate.Text = "Create Server";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btReturn
            // 
            btReturn.Font = new Font("Showcard Gothic", 24F);
            btReturn.Location = new Point(222, 541);
            btReturn.Margin = new Padding(2);
            btReturn.Name = "btReturn";
            btReturn.Size = new Size(392, 52);
            btReturn.TabIndex = 11;
            btReturn.Text = "Return";
            btReturn.UseVisualStyleBackColor = true;
            btReturn.Click += btReturn_Click;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Showcard Gothic", 70F);
            lblTitle.ForeColor = Color.White;
            lblTitle.ImageAlign = ContentAlignment.MiddleRight;
            lblTitle.Location = new Point(2, 11);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1253, 145);
            lblTitle.TabIndex = 15;
            lblTitle.Text = "Online Configuration";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPlayerCount
            // 
            lblPlayerCount.Font = new Font("Showcard Gothic", 40F);
            lblPlayerCount.ForeColor = Color.White;
            lblPlayerCount.ImageAlign = ContentAlignment.MiddleRight;
            lblPlayerCount.Location = new Point(296, 310);
            lblPlayerCount.Margin = new Padding(2, 0, 2, 0);
            lblPlayerCount.Name = "lblPlayerCount";
            lblPlayerCount.Size = new Size(318, 74);
            lblPlayerCount.TabIndex = 16;
            lblPlayerCount.Text = "Player:";
            lblPlayerCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCounter
            // 
            lblCounter.Font = new Font("Showcard Gothic", 40F);
            lblCounter.ForeColor = Color.White;
            lblCounter.ImageAlign = ContentAlignment.MiddleRight;
            lblCounter.Location = new Point(582, 310);
            lblCounter.Margin = new Padding(2, 0, 2, 0);
            lblCounter.Name = "lblCounter";
            lblCounter.Size = new Size(68, 74);
            lblCounter.TabIndex = 17;
            lblCounter.Text = "2";
            lblCounter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnMinus
            // 
            btnMinus.Font = new Font("Showcard Gothic", 30F);
            btnMinus.Location = new Point(673, 318);
            btnMinus.Margin = new Padding(2);
            btnMinus.Name = "btnMinus";
            btnMinus.Size = new Size(85, 64);
            btnMinus.TabIndex = 39;
            btnMinus.Text = "-";
            btnMinus.UseVisualStyleBackColor = true;
            btnMinus.Click += btnMinus_Click;
            // 
            // btnPlus
            // 
            btnPlus.Font = new Font("Showcard Gothic", 30F);
            btnPlus.Location = new Point(774, 318);
            btnPlus.Margin = new Padding(2);
            btnPlus.Name = "btnPlus";
            btnPlus.Size = new Size(85, 64);
            btnPlus.TabIndex = 38;
            btnPlus.Text = "+";
            btnPlus.UseVisualStyleBackColor = true;
            btnPlus.Click += btnPlus_Click;
            // 
            // OnlineConfig
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 46);
            Controls.Add(btnMinus);
            Controls.Add(btnPlus);
            Controls.Add(lblCounter);
            Controls.Add(lblPlayerCount);
            Controls.Add(lblTitle);
            Controls.Add(btnCreate);
            Controls.Add(btReturn);
            Name = "OnlineConfig";
            Size = new Size(1255, 728);
            ResumeLayout(false);
        }

        #endregion

        private Button btnCreate;
        private Button btReturn;
        private Label lblTitle;
        private Label lblPlayerCount;
        private Label lblCounter;
        private Button btnMinus;
        private Button btnPlus;
    }
}
