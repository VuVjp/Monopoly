namespace Monopoly;

partial class Menu
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
        lblTitle = new Label();
        btnSingle = new Button();
        btnOnline = new Button();
        btnAbout = new Button();
        btnQuit = new Button();
        button1 = new Button();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Showcard Gothic", 120F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lblTitle.ForeColor = Color.Gold;
        lblTitle.Location = new Point(73, 71);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(1432, 297);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Monopoly";
        // 
        // btnSingle
        // 
        btnSingle.Font = new Font("Arial Rounded MT Bold", 24F);
        btnSingle.Location = new Point(226, 415);
        btnSingle.Name = "btnSingle";
        btnSingle.Size = new Size(1068, 86);
        btnSingle.TabIndex = 1;
        btnSingle.Text = "Singleplayer";
        btnSingle.UseVisualStyleBackColor = true;
        btnSingle.Click += btnSingle_Click;
        // 
        // btnOnline
        // 
        btnOnline.Font = new Font("Arial Rounded MT Bold", 24F);
        btnOnline.Location = new Point(226, 516);
        btnOnline.Name = "btnOnline";
        btnOnline.Size = new Size(1068, 86);
        btnOnline.TabIndex = 2;
        btnOnline.Text = "Multiplayer";
        btnOnline.UseVisualStyleBackColor = true;
        // 
        // btnAbout
        // 
        btnAbout.Font = new Font("Arial Rounded MT Bold", 24F);
        btnAbout.Location = new Point(226, 726);
        btnAbout.Name = "btnAbout";
        btnAbout.Size = new Size(525, 86);
        btnAbout.TabIndex = 3;
        btnAbout.Text = "About Us";
        btnAbout.UseVisualStyleBackColor = true;
        // 
        // btnQuit
        // 
        btnQuit.Font = new Font("Arial Rounded MT Bold", 24F);
        btnQuit.Location = new Point(769, 726);
        btnQuit.Name = "btnQuit";
        btnQuit.Size = new Size(525, 86);
        btnQuit.TabIndex = 4;
        btnQuit.Text = "Quit Game";
        btnQuit.UseVisualStyleBackColor = true;
        btnQuit.Click += btnQuit_Click;
        // 
        // button1
        // 
        button1.Font = new Font("Arial Rounded MT Bold", 24F);
        button1.Location = new Point(226, 617);
        button1.Name = "button1";
        button1.Size = new Size(1068, 86);
        button1.TabIndex = 5;
        button1.Text = "How To Play";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // Menu
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 136, 229);
        ClientSize = new Size(1517, 917);
        Controls.Add(button1);
        Controls.Add(btnQuit);
        Controls.Add(btnAbout);
        Controls.Add(btnOnline);
        Controls.Add(btnSingle);
        Controls.Add(lblTitle);
        ForeColor = SystemColors.ControlText;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "Menu";
        SizeGripStyle = SizeGripStyle.Hide;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Form1";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Button btnSingle;
    private Button btnOnline;
    private Button btnAbout;
    private Button btnQuit;
    private Button button1;
}
