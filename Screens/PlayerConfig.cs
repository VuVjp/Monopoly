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
    public partial class PlayerConfig : UserControl
    {
        private Monopoly monopoly;
        public PlayerConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new Game(monopoly));
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new GameConfig(monopoly));
        }
    }
}
