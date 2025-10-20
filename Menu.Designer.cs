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
        btnMulti = new Button();
        btnAbout = new Button();
        btnQuit = new Button();
        button1 = new Button();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Showcard Gothic", 72F);
        lblTitle.ForeColor = Color.Gold;
        lblTitle.Location = new Point(124, 53);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(859, 177);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Monopoly";
        // 
        // btnSingle
        // 
        btnSingle.Font = new Font("Arial Rounded MT Bold", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnSingle.Location = new Point(222, 282);
        btnSingle.Name = "btnSingle";
        btnSingle.Size = new Size(637, 72);
        btnSingle.TabIndex = 1;
        btnSingle.Text = "Singleplayer";
        btnSingle.UseVisualStyleBackColor = true;
        btnSingle.Click += btnSingle_Click;
        // 
        // btnMulti
        // 
        btnMulti.Font = new Font("Arial Rounded MT Bold", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnMulti.Location = new Point(222, 360);
        btnMulti.Name = "btnMulti";
        btnMulti.Size = new Size(637, 72);
        btnMulti.TabIndex = 2;
        btnMulti.Text = "Multiplayer";
        btnMulti.UseVisualStyleBackColor = true;
        // 
        // btnAbout
        // 
        btnAbout.Font = new Font("Arial Rounded MT Bold", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnAbout.Location = new Point(222, 516);
        btnAbout.Name = "btnAbout";
        btnAbout.Size = new Size(310, 72);
        btnAbout.TabIndex = 3;
        btnAbout.Text = "About Us";
        btnAbout.UseVisualStyleBackColor = true;
        // 
        // btnQuit
        // 
        btnQuit.Font = new Font("Arial Rounded MT Bold", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnQuit.Location = new Point(549, 516);
        btnQuit.Name = "btnQuit";
        btnQuit.Size = new Size(310, 72);
        btnQuit.TabIndex = 4;
        btnQuit.Text = "Quit Game";
        btnQuit.UseVisualStyleBackColor = true;
        btnQuit.Click += btnQuit_Click;
        // 
        // button1
        // 
        button1.Font = new Font("Arial Rounded MT Bold", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
        button1.Location = new Point(222, 438);
        button1.Name = "button1";
        button1.Size = new Size(637, 72);
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
        ClientSize = new Size(1120, 659);
        Controls.Add(button1);
        Controls.Add(btnQuit);
        Controls.Add(btnAbout);
        Controls.Add(btnMulti);
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
    private Button btnMulti;
    private Button btnAbout;
    private Button btnQuit;
    private Button button1;
}
