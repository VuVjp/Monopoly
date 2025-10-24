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
        List<Bitmap> listImage = new List<Bitmap>()
        {Properties.Resources.Dice1,
        Properties.Resources.Dice2,
        Properties.Resources.Dice3,
        Properties.Resources.Dice4,
        Properties.Resources.Dice5,
        Properties.Resources.Dice6};
        public int X1;
        int rollTime = 0;
        int endTime;
        bool Rolling = false;
        public Game()
        {
            InitializeComponent();
            LoadForm();
        }
        private void LoadForm()
        {
            Dice1.Click += RollDice;
            Dice2.Click += RollDice;
            RollTime.Interval = 300;
            endTime = 2000;
            X1 = Dice1.Location.X;

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
            pcb.Cursor = Cursors.Hand;
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
        }
        private void pcb_MouseLeave(object sender, EventArgs e)
        {
            var pcb = (PictureBox)sender;
            pcb.Cursor = Cursors.Hand;
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
        }
        private void RollDice(object sender, EventArgs e)
        {
            if (Rolling == false)
            {
                RollTime.Start();
                Rolling = true;
                int[] result = new int[2];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = GameManager.RollDice();
                }
                Dice1.BackgroundImage = listImage[result[0]];
                Dice2.BackgroundImage = listImage[result[1]];
            }

        }

        private void RollTime_Tick(object sender, EventArgs e)
        {
            rollTime += RollTime.Interval;
            Random x = new Random();
            Random y = new Random();
            Dice1.BackgroundImage = listImage[x.Next(6)];
            Dice2.BackgroundImage = listImage[y.Next(6)];
            if (Dice1.Location.X == X1)
            {
                Dice1.Location = new Point(Dice1.Location.X + 2, Dice1.Location.Y);
                Dice2.Location = new Point(Dice2.Location.X + 2, Dice2.Location.Y);
            }
            else if (Dice1.Location.X < X1)
            {
                Dice1.Location = new Point(Dice1.Location.X + 4, Dice1.Location.Y);
                Dice2.Location = new Point(Dice2.Location.X + 4, Dice2.Location.Y);
            }
            else if (Dice1.Location.X > X1)
            {
                Dice1.Location = new Point(Dice1.Location.X - 2, Dice1.Location.Y);
                Dice2.Location = new Point(Dice2.Location.X - 2, Dice2.Location.Y);
            }
            if (rollTime >= endTime)
            {
                RollTime.Stop();
                rollTime = 0;
                Rolling = false;
            }
        }
    }
}
