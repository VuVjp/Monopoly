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
        private WaveOutEvent bgMusic;
        private WaveFileReader bgReader;
        public MainMenu(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            InitAudio();
        }
        private void InitAudio()
        {
            bgReader = new WaveFileReader(new MemoryStream(Properties.Resources.MenuMusic));
            bgMusic = new WaveOutEvent();
            bgMusic.Init(bgReader);
            bgMusic.Volume = 0.2f;
            bgMusic.PlaybackStopped += BgMusic_LoopHandler;
            bgMusic.Play();
        }
        private void BgMusic_LoopHandler(object sender, StoppedEventArgs e)
        {
            bgReader.Position = 0;
            bgMusic.Play();
        }
        private void StopMusic()
        {
            if (bgMusic != null)
            {
                bgMusic.PlaybackStopped -= BgMusic_LoopHandler;
                bgMusic.Stop();
                bgMusic.Dispose();
                bgReader.Dispose();
                bgMusic = null;
                bgReader = null;
            }
        }
        private void btnSingle_Click(object sender, EventArgs e)
        {
            PlayerManager.isOnline = false;
            StopMusic();
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            monopoly.Close();
        }

        private void btnOnline_Click(object sender, EventArgs e)
        {
            StopMusic();
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