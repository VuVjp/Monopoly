using Monopoly.Scripts;
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
    public partial class Game : UserControl
    {
        private Monopoly monopoly;
        public Game(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            LoadPlayerInfo();
        }
        private void LoadPlayerInfo()
        {
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                switch (i)
                {
                    case 0:
                        pcbPlrAvt1.Image = PlayerManager.Players[i].Avt.Image;
                        lblPlayerName1.Text = PlayerManager.Players[i].Name;
                        lblPlayerMoney1.Text = "$" + PlayerManager.Players[i].PlayerMoney.ToString();
                        pnlPlr1.Visible = true;
                        break;
                    case 1:
                        pcbPlrAvt2.Image = PlayerManager.Players[i].Avt.Image;
                        lblPlayerName2.Text = PlayerManager.Players[i].Name;
                        lblPlayerMoney2.Text = "$" + PlayerManager.Players[i].PlayerMoney.ToString();
                        pnlPlr2.Visible = true;
                        break;
                    case 2:
                        pcbPlrAvt3.Image = PlayerManager.Players[i].Avt.Image;
                        lblPlayerName3.Text = PlayerManager.Players[i].Name;
                        lblPlayerMoney3.Text = "$" + PlayerManager.Players[i].PlayerMoney.ToString();
                        pnlPlr3.Visible = true;
                        break;
                    case 3:
                        pcbPlrAvt4.Image = PlayerManager.Players[i].Avt.Image;
                        lblPlayerName4.Text = PlayerManager.Players[i].Name;
                        lblPlayerMoney4.Text = "$" + PlayerManager.Players[i].PlayerMoney.ToString();
                        pnlPlr4.Visible = true;
                        break;
                }
            }
        }

        private void pcbReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new MainMenu(monopoly));
        }
        private void pcb_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
        }
        private void pcb_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
        }
    }
}
