using Monopoly.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly.Screens
{
    public partial class OnlineConfig : UserControl
    {
        private Monopoly monopoly = null!;
        public OnlineConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            PlayerManager.playerCount = int.Parse(lblCounter.Text);
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblCounter.Text) > 2)
            {
                lblCounter.Text = (int.Parse(lblCounter.Text) - 1).ToString();
            }
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblCounter.Text) < 4)
            {
                lblCounter.Text = (int.Parse(lblCounter.Text) + 1).ToString();
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            PlayerManager.playerCount = int.Parse(lblCounter.Text);
            monopoly.LoadScreen(new PlayerConfig(monopoly));
        }

        private void btReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new GameConfig(monopoly));
        }
    }
}
