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
    public partial class Game : Form
    {

        public Game()
        {
            InitializeComponent();
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.menu.Close();
        }

        private void pcbReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.menu.Show();
        }

        private void pcbSave_Click(object sender, EventArgs e)
        {

        }

        private void pcbReturn_MouseEnter(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(180, 180);
        }

        private void pcbReturn_MouseLeave(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(169, 169);
        }

        private void pcbSave_MouseEnter(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(180, 180);
        }

        private void pcbSave_MouseLeave(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(169, 169);
        }
    }
}
