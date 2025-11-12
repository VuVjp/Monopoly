namespace Monopoly.Screens
{
    partial class FindServer
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
            btnJoin = new Button();
            btReturn = new Button();
            tbxIP = new TextBox();
            txbPort = new TextBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
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
            lblTitle.TabIndex = 18;
            lblTitle.Text = "Join Directly";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnJoin
            // 
            btnJoin.Font = new Font("Showcard Gothic", 24F);
            btnJoin.Location = new Point(628, 603);
            btnJoin.Margin = new Padding(2);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(392, 52);
            btnJoin.TabIndex = 17;
            btnJoin.Text = "Join Server";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += btnJoin_Click;
            // 
            // btReturn
            // 
            btReturn.Font = new Font("Showcard Gothic", 24F);
            btReturn.Location = new Point(221, 603);
            btReturn.Margin = new Padding(2);
            btReturn.Name = "btReturn";
            btReturn.Size = new Size(392, 52);
            btReturn.TabIndex = 16;
            btReturn.Text = "Return";
            btReturn.UseVisualStyleBackColor = true;
            btReturn.Click += btReturn_Click;
            // 
            // tbxIP
            // 
            tbxIP.Font = new Font("Segoe UI", 20F);
            tbxIP.Location = new Point(424, 271);
            tbxIP.Name = "tbxIP";
            tbxIP.Size = new Size(555, 43);
            tbxIP.TabIndex = 19;
            // 
            // txbPort
            // 
            txbPort.Font = new Font("Segoe UI", 20F);
            txbPort.Location = new Point(424, 398);
            txbPort.Name = "txbPort";
            txbPort.Size = new Size(555, 43);
            txbPort.TabIndex = 20;
            // 
            // label1
            // 
            label1.Font = new Font("Showcard Gothic", 40F);
            label1.ForeColor = Color.White;
            label1.ImageAlign = ContentAlignment.MiddleRight;
            label1.Location = new Point(309, 236);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(110, 114);
            label1.TabIndex = 21;
            label1.Text = "IP:";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Font = new Font("Showcard Gothic", 40F);
            label2.ForeColor = Color.White;
            label2.ImageAlign = ContentAlignment.MiddleRight;
            label2.Location = new Point(221, 363);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(198, 114);
            label2.TabIndex = 22;
            label2.Text = "Port:";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FindServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 46);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txbPort);
            Controls.Add(tbxIP);
            Controls.Add(lblTitle);
            Controls.Add(btnJoin);
            Controls.Add(btReturn);
            Name = "FindServer";
            Size = new Size(1255, 728);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Button btnJoin;
        private Button btReturn;
        private TextBox tbxIP;
        private TextBox txbPort;
        private Label label1;
        private Label label2;
    }
}
