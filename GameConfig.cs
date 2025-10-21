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
    public partial class GameConfig : Form
    {
        public GameConfig()
        {
            InitializeComponent();
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.menu.Show();
        }

        private void GameConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            this.Hide();
            if (!Forms.isOnline)
                Forms.singleConfig.Show();
        }
    }
}
