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
        List<Bitmap> listImage = new List<Bitmap>()
        {Properties.Resources.Dice1,
        Properties.Resources.Dice2,
        Properties.Resources.Dice3,
        Properties.Resources.Dice4,
        Properties.Resources.Dice5,
        Properties.Resources.Dice6};

        List<Label> labels;
        public int X1;
        int rollTime = 0;
        int endTime;
        bool Rolling = false;
        private Monopoly monopoly;
        public Game(Monopoly monopoly)
        {
           
            InitializeComponent();
            labels = new List<Label>{ pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8,
            pos9, pos10, pos11, pos12, pos13, pos14, pos15, pos16, pos17, pos18, pos19,
            pos20, pos21, pos22, pos23, pos24, pos25, pos26, pos27, pos28, pos29, pos30, pos31, pos32}
          ;
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
            LoadPlayerInBoard();
            Dice1.Click += RollDice;
            Dice2.Click += RollDice;
            RollTime.Tick += RollTime_Tick;
            RollTime.Interval = 300;
            endTime = 2000;
            X1 = Dice1.Location.X;
           
        }
        private void RollDice(object sender, EventArgs e)
        {
            if (Rolling == false)
            {
                Rolling = true;
                RollTime.Start();
                rollTime = 0;
            }

        }
        private void RollTime_Tick(object sender, EventArgs e)
        {
            if (rollTime >= endTime)
            {
                RollTime.Stop();
                Rolling = false;
                int[] result = new int[2];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = GameManager.RollDice();
                }
                Dice1.BackgroundImage = listImage[result[0]-1];
                Dice2.BackgroundImage = listImage[result[1]-1];
            }
            else
            {
                rollTime += RollTime.Interval;
                Random x = new Random();
                Random y = new Random();
                Dice1.BackgroundImage = listImage[x.Next(6)];
                Dice2.BackgroundImage = listImage[y.Next(6)];
                if (Dice1.Location.X == X1)
                {
                    Dice1.Location = new Point(Dice1.Location.X + 3, Dice1.Location.Y);
                    Dice2.Location = new Point(Dice2.Location.X + 3, Dice2.Location.Y);
                }
                else if (Dice1.Location.X < X1)
                {
                    Dice1.Location = new Point(Dice1.Location.X + 6, Dice1.Location.Y);
                    Dice2.Location = new Point(Dice2.Location.X + 6, Dice2.Location.Y);
                }
                else if (Dice1.Location.X > X1)
                {
                    Dice1.Location = new Point(Dice1.Location.X - 3, Dice1.Location.Y);
                    Dice2.Location = new Point(Dice2.Location.X - 3, Dice2.Location.Y);
                }
            }
        }
        private void LoadPlayerInBoard()
        {
            int dist = 5;
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                if (i < 1)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(45, 50);
                    pictureBox.Location = new Point(labels[0].Location.X-25, labels[0].Location.Y-30);
                    pictureBox.BackgroundImage = Properties.Resources.VuVjp;
                    pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
                    pnl1.Controls.Add(pictureBox);
                    pictureBox.BringToFront();
                }
                else
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(45, 50);
                    pictureBox.Location = new Point(labels[0].Location.X-25+dist, labels[0].Location.Y-30);
                    pictureBox.BackgroundImage = Properties.Resources.VuVjp;
                    pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
                    pnl1.Controls.Add(pictureBox);
                   pictureBox.BringToFront();
                 
                    dist += 5;
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
