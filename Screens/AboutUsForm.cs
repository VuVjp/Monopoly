using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly.Screens
{
    public partial class AboutUsForm : Form
    {
        public AboutUsForm()
        {
            InitializeComponent();
            this.Text = "About Us";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            Panel panel = new Panel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            Label lblTitle = new Label()
            {
                Text = "ABOUT US",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10)
            };
            Label lblContent = new Label()
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                MaximumSize = new Size(550, 0),
                Location = new Point(25, 80),
                Text =
@"We are a team of 4 developers who worked together to build this Monopoly-style board game.

OUR TEAM
1. **Nguyễn Hoàng Nguyên Vũ**
   - Designed and implemented WinForms UI
   - Developed Socket communication and client management
   - Built the main game loop and game logic
   - Optimized UI and UX for smooth gameplay

2. **Phạm Minh Tình**
   - Designed WinForms interface
   - Implemented sound and audio effects
   - Created offline save/load system
   - Contributed to game logic and event handling

3. **Trần Đình Bắc**
   - Conducted surveys and user research
   - Designed parts of the WinForms UI
   - Developed the server search and connection features

4. **Đào Đức Long**
   - Assisted with testing, debugging, and documentation
   - Supported UI adjustments and gameplay balance

Together, we focused on combining functionality, interactivity, and user experience to create an engaging Monopoly-inspired board game playable both offline and over LAN."
            };
            panel.Controls.Add(lblContent);
            panel.Controls.Add(lblTitle);
            this.Controls.Add(panel);
        }
    }
}