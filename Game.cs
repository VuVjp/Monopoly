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
            LoadForm();

        }
        private void LoadForm()
        {

            if (Forms.isOnline)
            {

                pcbChat.Visible = true;
            }
            else
            {
                pcbChat.Visible = false;
            }
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }

        private void pcbReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.menu.Show();
        }

        private void pcb_MouseEnter(object sender, EventArgs e)
        {
            var pcb = (PictureBox)sender;
            pcb.Size = new Size(110, 110);
        }
        private void pcb_MouseLeave(object sender, EventArgs e)
        {
            var pcb = (PictureBox)sender;
            pcb.Size = new Size(100, 100);
        }
    }
}
