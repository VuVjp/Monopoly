using Monopoly.Screens;
using Monopoly.Scripts;
using Monopoly.Scripts.LookUp;
using Monopoly.Scripts.Sockets;
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

namespace Monopoly
{
    public partial class Monopoly : Form
    {
        private WaveOutEvent bgMusic;
        private WaveFileReader bgReader;
        private string curMusic = null!;
        public Monopoly()
        {
            InitializeComponent();
            LoadScreen(new MainMenu(this));
        }
        public void LoadScreen(UserControl Screen)
        {
            pnlMain.Controls.Clear();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Controls.Add(Screen);
            if(Screen.Name != "Game" && Screen.Name != "PlayerConfig")
            {
                Screen.Name = "Menu";
            }
            if (Screen.Name == curMusic) return;
            if (Screen.Name == "Game")
            {
                curMusic = Screen.Name;
                StopMusic();
                InitAudio(3);
            }
            else if (Screen.Name == "PlayerConfig")
            {
                curMusic = Screen.Name;
                StopMusic();
                InitAudio(2);
            }
            else
            {
                curMusic = Screen.Name;
                StopMusic();
                InitAudio(1);
            }
        }

        private void Monopoly_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Invoke(() => SocketServerManager.bcast?.Stop());
            StopMusic();
            Environment.Exit(0);
        }
        private void InitAudio(int type)
        {
            switch (type)
            {
                case 1:
                    {
                        bgReader = new WaveFileReader(new MemoryStream(Properties.Resources.MenuMusic));
                        break;
                    }
                case 2:
                    {
                        bgReader = new WaveFileReader(Properties.Resources.Lobby);
                        break;
                    }
                case 3:
                    {
                        bgReader = new WaveFileReader(new MemoryStream(Properties.Resources.BackGroundMusic));
                        break;
                    }
            }
            bgMusic = new WaveOutEvent();
            bgMusic.Init(bgReader);
            bgMusic.Volume = 0.3f;
            bgMusic.PlaybackStopped += BgMusic_LoopHandler!;
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
                bgMusic.PlaybackStopped -= BgMusic_LoopHandler!;
                bgMusic.Stop();
                bgMusic.Dispose();
                bgReader.Dispose();
            }
        }
    }
}
