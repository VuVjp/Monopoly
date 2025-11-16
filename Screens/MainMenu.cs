using Monopoly.Scripts;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly.Screens
{
    public partial class MainMenu : UserControl
    {

        private Monopoly monopoly;
        public MainMenu(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            lblTitle.Location = new Point(this.Size.Width/9, lblTitle.Location.Y);
        }

        private void btnSingle_Click(object sender, EventArgs e)
        {
            PlayerManager.isOnline = false;
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            monopoly.Close();
        }

        private void btnOnline_Click(object sender, EventArgs e)
        {
            PlayerManager.isOnline = true;
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnHowToPlay_Click(object sender, EventArgs e)
        {
            HowToPlayForm howToPlayForm = new HowToPlayForm();
            howToPlayForm.ShowDialog();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutUsForm aboutUsForm = new AboutUsForm();
            aboutUsForm.ShowDialog();
        }
    }
}