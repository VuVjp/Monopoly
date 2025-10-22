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
    public partial class GameConfig : UserControl
    {
        private Monopoly monopoly;
        public GameConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new Game(monopoly));
        }
    }
}
