using Monopoly.Scripts.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly
{
    public partial class Monopoly : Form
    {
        public Monopoly()
        {
            InitializeComponent();
            LoadScreen(new Screens.MainMenu(this));
        }
        public void LoadScreen(UserControl Screen)
        {
            pnlMain.Controls.Clear();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Controls.Add(Screen);
        }
    }
}
