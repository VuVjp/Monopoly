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
    public partial class MainMenu : UserControl
    {
        private Monopoly monopoly;
        public MainMenu(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
        }

        private void btnSingle_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            monopoly.Close();
        }
    }
}
