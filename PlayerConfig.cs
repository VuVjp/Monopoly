using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly
{
    public partial class PlayerConfig : Form
    {
        PictureBox selectedPlr = null!;
        PictureBox curImg = null!;
        List<PictureBox> plrPics = null!;
        List<PictureBox> plrImgs = null!;

        public PlayerConfig()
        {
            InitializeComponent();
            FormLoader();
        }
        private void FormLoader()
        {
            int counter = 1;
            plrPics = new List<PictureBox>() { pcbPlr1, pcbPlr2, pcbPlr3, pcbPlr4 };
            plrImgs = new List<PictureBox>() { pcbAvt1, pcbAvt2, pcbAvt3, pcbAvt4, pcbAvt5, pcbAvt6, pcbAvt7, pcbAvt8 };
            foreach (PictureBox img in plrImgs)
            {
                img.Image = (Image)Properties.Resources.ResourceManager.GetObject("plr" +
                    counter++)!;
                img.Click += Img_Click;
            }
            if (Forms.isOnline)
            {
                if (!Forms.isAdmin)
                {
                    btnPlay.Enabled = false;
                    btnPlus.Enabled = false;
                    btnMinus.Enabled = false;
                    //txbPlrCounter.Text = Socket nhận từ server
                    //SelectPlrUI(); socket nhận từ server
                }
                FormLoaderLoadFormOnlineMode();
            }
            else
            {
                FormLoaderLoadFormOfflineMode();
            }
        }
        private void FormLoaderLoadFormOnlineMode()
        {
            HidePics(int.Parse(txbPlrCounter.Text), plrPics.Count);
            ShowPics(2, int.Parse(txbPlrCounter.Text));
        }

        private void FormLoaderLoadFormOfflineMode()
        {
            selectedImg.Visible = false;
            selectedPlr = pcbPlr1;
            
            foreach (PictureBox plr in plrPics) plr.Click += pcbPlr_Click!;
            HidePics(int.Parse(txbPlrCounter.Text), plrPics.Count);
        }

        private void Img_Click(object? sender, EventArgs e)
        {
            selectedImg.Visible = true;
            curImg = (PictureBox)sender!;
            if (curImg.Tag == null && selectedPlr.Tag == null)
            {
                curImg.Tag = selectedPlr;
                selectedPlr.Tag = curImg;

                selectedPlr.Image = curImg.Image;
                //selectedPlr.SizeMode = PictureBoxSizeMode.StretchImage;
                selectedImg.Location = new Point(curImg.Location.X - 10, curImg.Location.Y - 10);
            }
            if (curImg.Tag == null && selectedPlr.Tag != null)
            {
                PictureBox oldImg = (PictureBox)selectedPlr.Tag;
                oldImg.Tag = null;
                curImg.Tag = selectedPlr;
                selectedPlr.Tag = curImg;

                selectedPlr.Image = curImg.Image;
                selectedPlr.SizeMode = PictureBoxSizeMode.StretchImage;
                selectedImg.Location = new Point(curImg.Location.X - 10, curImg.Location.Y - 10);
            }
        }

        public void HidePics(int index, int length)
        {
            for (int i = index; i < length; i++)
            {
                switch (i)
                {
                    case 2:
                        if (selectedPlr == pcbPlr3)
                        {
                            if(selectedPlr.Tag != null)
                            {
                                PictureBox oldImg = (PictureBox)selectedPlr.Tag!;
                                oldImg.Tag = null;
                                selectedPlr.Tag = null;
                            }
                            SelectPlrUI(pcbPlr2);
                            pcbPlr3.Image = Properties.Resources.user;
                        }
                        pcbPlr3.Visible = false;
                        break;
                    case 3:
                        if (selectedPlr == pcbPlr4)
                        {
                            if (selectedPlr.Tag != null)
                            {
                                PictureBox oldImg = (PictureBox)selectedPlr.Tag!;
                                oldImg.Tag = null;
                                selectedPlr.Tag = null;
                            }
                            SelectPlrUI(pcbPlr3);
                            pcbPlr4.Image = Properties.Resources.user;
                        }
                        pcbPlr4.Visible = false;
                        break;
                }
            }
        }

        public void ShowPics(int index, int length)
        {
            for (int i = index; i < length; i++)
            {
                switch (i)
                {
                    case 2:
                        pcbPlr3.Visible = true;
                        break;
                    case 3:
                        pcbPlr4.Visible = true;
                        break;
                }
            }
        }

        public void SelectPlrUI(PictureBox selectedPlr)
        {
            selectedUI.Visible = true;
            this.selectedPlr = selectedPlr;
            selectedUI.Location = new Point(selectedPlr.Location.X - 10, selectedPlr.Location.Y - 10);
        }

        private void SingleConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }
        private void pcbPlr_Click(object sender, EventArgs e)
        {
            SelectPlrUI((PictureBox)sender);
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.gameConfig.Show();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.game.Show();
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (int.Parse(txbPlrCounter.Text) < 4)
            {
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) + 1).ToString();
                ShowPics(int.Parse(txbPlrCounter.Text) - 1, int.Parse(txbPlrCounter.Text));
            }
            else
                return;
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (int.Parse(txbPlrCounter.Text) > 2)
            {
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) - 1).ToString();
                HidePics(int.Parse(txbPlrCounter.Text), 4);
            }
            else
                return;
        }
    }
}
