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

        private void pcbSave_Click(object sender, EventArgs e)
        {

        }

        private void pcbReturn_MouseEnter(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(110, 110);
        }

        private void pcbReturn_MouseLeave(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(100, 100);
        }

        private void pcbSave_MouseEnter(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(110, 110);
        }

        private void pcbSave_MouseLeave(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(100, 100);
        }

        private void pcbChat_MouseHover(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(110, 110);
        }

        private void pcbChat_MouseEnter(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(110, 110);
        }

        private void pcbChat_MouseLeave(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(100, 100);
        }

        private void pcbInfo_DragEnter(object sender, DragEventArgs e)
        {
            pcbInfo.Size = new Size(110, 110);
        }

        private void pcbInfo_MouseEnter(object sender, EventArgs e)
        {
            pcbInfo.Size = new Size(110, 110);
        }

        private void pcbInfo_MouseLeave(object sender, EventArgs e)
        {
            pcbInfo.Size = new Size(100, 100);
        }
    }
}
