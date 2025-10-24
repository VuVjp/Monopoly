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
    public partial class PlayerConfig : UserControl
    {
        private Monopoly monopoly;
        List<PictureBox> plrImgs = null!;
        List<PictureBox> playerBoxes = null!;

        public PlayerConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            FormLoader();
        }

        private void FormLoader()
        {
            plrImgs = new List<PictureBox>() { pcbAvt1, pcbAvt2, pcbAvt3, pcbAvt4, pcbAvt5, pcbAvt6, pcbAvt7, pcbAvt8 };
            playerBoxes = new List<PictureBox> { pcbPlr1, pcbPlr2, pcbPlr3, pcbPlr4 };
            int counter = 1;
            foreach (PictureBox img in plrImgs)
            {
                img.Image = (Image)Properties.Resources.ResourceManager.GetObject("plr" +
                    counter++)!;
                img.Click += pcb_AvtClick!;
                img.MouseEnter += pcb_AvtMouseEnter!;
                img.MouseLeave += pcb_AvtMouseLeave!;
            }
            foreach(PictureBox pcb in playerBoxes)
            {
                pcb.Image = Properties.Resources.user;
            }
            if (PlayerManager.isOnline)
            {
                OnlineModeSetup();
            }
            else
            {
                OfflineModeSetup();
            }
        }

        private void OnlineModeSetup()
        {
            if (PlayerManager.isAdmin)
            {
                btnPlay.Enabled = false;
                btnPlus.Enabled = false;
                btnMinus.Enabled = false;
                //txbPlrCounter.Text = Socket nhận từ server
                //SelectPlrUI(); socket nhận từ server
            }
        }
        private void OfflineModeSetup()
        {
            PlayerManager.curPlayer = playerBoxes[0];
            foreach (PictureBox pcb in playerBoxes)
            {
                pcb.MouseEnter += pcb_PlayerMouseEnter!;
                pcb.MouseLeave += pcb_PlayerMouseLeave!;
                pcb.Click += pcb_PlayerClick!;
            }
        }

        private void pcb_AvtClick(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (PlayerManager.curPlayer == null) return;
            if (PlayerManager.curPlayer.Tag != null && pcb.Tag == null)
            {
                PictureBox oldImg = (PictureBox)PlayerManager.curPlayer.Tag;
                oldImg.Tag = null;
                oldImg.BackgroundImage = null;
                pcb.Tag = PlayerManager.curPlayer;
                pcb.BackgroundImage = Properties.Resources.Ban;
                PlayerManager.curPlayer.Tag = pcb;
                PlayerManager.curPlayer.Image = pcb.Image;
                LeaveAnimation(pcb);
                return;
            }
            if (pcb.Tag == null)
            {
                PlayerManager.curPlayer.Tag = pcb.Tag;
                pcb.Tag = PlayerManager.curPlayer;
                pcb.BackgroundImage = Properties.Resources.Ban;
                PlayerManager.curPlayer.Image = pcb.Image;
                PlayerManager.curPlayer.Tag = pcb;
                LeaveAnimation(pcb);
                return;
            }
        }
        private void pcb_AvtMouseEnter(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (pcb.Tag != null) return;
            EnterAnimation(pcb);
        }
        private void pcb_AvtMouseLeave(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (pcb.Tag != null) return;
            LeaveAnimation(pcb);
        }

        private void EnterAnimation(PictureBox pcb)
        {
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
            pcb.Location = new Point(pcb.Location.X - 5, pcb.Location.Y - 5);
            selectedImg.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedImg.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void LeaveAnimation(PictureBox pcb)
        {
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
            pcb.Location = new Point(pcb.Location.X + 5, pcb.Location.Y + 5);
            selectedImg.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedImg.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void pcb_PlayerClick(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            PlayerManager.curPlayer = pcb;
            selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void pcb_PlayerMouseEnter(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
            pcb.Location = new Point(pcb.Location.X - 5, pcb.Location.Y - 5);
            if (PlayerManager.curPlayer == pcb)
            {
                selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
                selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
            }
        }

        private void pcb_PlayerMouseLeave(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
            pcb.Location = new Point(pcb.Location.X + 5, pcb.Location.Y + 5);
            if (PlayerManager.curPlayer == pcb)
            {
                selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
                selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            PlayerManager.Players = new List<PlayerInfo>();
            for (int i = 0; i < int.Parse(txbPlrCounter.Text); i++)
            {
                var pcb = playerBoxes[i];
                if (pcb.Tag == null)
                {
                    MessageBox.Show("All players must select an avatar!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var pictureBox = (PictureBox)pcb.Tag!;
                PlayerInfo plr = new PlayerInfo()
                {
                    Name = "Player " + (i + 1),
                    Avt = pictureBox,
                    Color = PlayerManager.colorMapping[pictureBox.Name]
                };
                PlayerManager.Players.Add(plr);
            }
            PlayerManager.playerCount = PlayerManager.Players.Count;
            monopoly.LoadScreen(new Game(monopoly));
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            if(int.Parse(txbPlrCounter.Text) < 4)
            {
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) + 1).ToString();
                playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Visible = true;
            }
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (int.Parse(txbPlrCounter.Text) > 2)
            {
                if (PlayerManager.curPlayer == playerBoxes[int.Parse(txbPlrCounter.Text) - 1])
                {
                    PlayerManager.curPlayer = playerBoxes[int.Parse(txbPlrCounter.Text) - 2];
                    selectedUI.Location = new Point(PlayerManager.curPlayer.Location.X - 10, PlayerManager.curPlayer.Location.Y - 10);
                    selectedUI.Size = new Size(PlayerManager.curPlayer.Size.Width + 20, PlayerManager.curPlayer.Size.Height + 20);
                }
                if(playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag != null)
                {
                    PictureBox oldImg = (PictureBox)playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag!;
                    oldImg.Tag = null;
                    oldImg.BackgroundImage = null;
                    playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Image = Properties.Resources.user;
                    playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag = null;
                }
                playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Visible = false;
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) - 1).ToString();
            }
        }
    }
}
