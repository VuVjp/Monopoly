using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly
{
    public partial class SingleConfig : Form
    {
        PictureBox selectedPlr = null!;
        PictureBox curImg = null!;

        public SingleConfig()
        {
            InitializeComponent();
            LoadForm();
        }

        private void LoadForm()
        {
            int counter = 1;
            selectedImg.Visible = false;
            selectedPlr = pcbPlr1;
            List<PictureBox> plrPics = new List<PictureBox>() { pcbPlr1, pcbPlr2, pcbPlr3, pcbPlr4 };
            List<PictureBox> plrImgs = new List<PictureBox>() { pcbAvt1, pcbAvt2, pcbAvt3, pcbAvt4, pcbAvt5, pcbAvt6, pcbAvt7, pcbAvt8};
            foreach (PictureBox img in plrImgs)
            {
                img.BackgroundImage = (Image)Properties.Resources.ResourceManager.GetObject("plr" + 
                    ++counter)!;
                img.Click += Img_Click;
            }
            foreach (PictureBox plr in plrPics) plr.Click += pcbPlr_Click!;
            HidePics(int.Parse(txbPlrCounter.Text), plrPics.Count);
        }

        private void Img_Click(object? sender, EventArgs e)
        {
            selectedImg.Visible = true;
            curImg = (PictureBox)sender!;
            selectedPlr.Image = curImg.BackgroundImage;
            selectedPlr.SizeMode = PictureBoxSizeMode.StretchImage;
            selectedImg.Location = new Point(curImg.Location.X - 10, curImg.Location.Y - 10);
        }

        private void HidePics(int index, int length)
        {
            for (int i = index; i < length; i++)
            {
                switch (i)
                {
                    case 2:
                        if (selectedPlr == pcbPlr3)
                            selectPlrUI(pcbPlr2);
                        pcbPlr3.Visible = false;
                        break;
                    case 3:
                        if (selectedPlr == pcbPlr4)
                            selectPlrUI(pcbPlr3);
                        pcbPlr4.Visible = false;
                        break;
                }
            }
        }

        private void ShowPics(int index, int length)
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

        private void selectPlrUI(PictureBox selectedPlr)
        {
            selectedUI.Visible = true;
            this.selectedPlr = selectedPlr;
            if (curImg != null)
            {
                selectedPlr.Image = curImg.Image;
                selectedPlr.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            selectedUI.Location = new Point(selectedPlr.Location.X - 10, selectedPlr.Location.Y - 10);
        }

        private void SingleConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }
        private void pcbPlr_Click(object sender, EventArgs e)
        {
            selectPlrUI((PictureBox)sender);
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
