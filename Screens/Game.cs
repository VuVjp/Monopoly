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
        #region Offline Properties
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
        bool extraTurn = false;
        private Monopoly monopoly;
        int currPlayer = 0;
        Dictionary<int, int> playerPos = new Dictionary<int, int>();
        Dictionary<int, int> Doubles = new Dictionary<int, int>(); 
        Dictionary<int, bool> inJail = new Dictionary<int, bool>();
        Dictionary<int, int> jailTurns = new Dictionary<int, int>();
        private Dictionary<int, (PictureBox Pcb, int CurrPos, int TargetPos)> moveLogic = new Dictionary<int, (PictureBox, int, int)>();
        #endregion
        #region Online Properties
        #endregion
        public Game(Monopoly monopoly)
        {

            InitializeComponent();
            this.monopoly = monopoly;
            labels = new List<Label>{ pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8,
            pos9, pos10, pos11, pos12, pos13, pos14, pos15, pos16, pos17, pos18, pos19,
            pos20, pos21, pos22, pos23, pos24, pos25, pos26, pos27, pos28, pos29, pos30, pos31, pos32, pos33, pos34, pos35, pos36}
            ;
            LoadPlayerInfo();
            LoadPlayerInBoard();
            if (!PlayerManager.isOnline)
            {
                for (int i = 0; i < PlayerManager.playerCount; i++)
                {
                    Doubles[i] = 0;
                    inJail[i] = false;
                    jailTurns[i] = 0;
                }

                Dice1.Click += RollDice!;
                Dice2.Click += RollDice!;
                RollTimer.Tick += RollTimer_Tick!;
                RollTimer.Interval = 300;
                endTime = 2000;
                X1 = Dice1.Location.X;

                label1.Text = PlayerManager.Players[currPlayer].Name;
                label1.ForeColor = PlayerManager.Players[currPlayer].Color;
            }
            else
            {

            }

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


        private void RollDice(object sender, EventArgs e)
        {
            if (inJail[currPlayer])
            {
                var player = PlayerManager.Players[currPlayer];
                DialogResult dr = MessageBox.Show(
                    $"{player.Name} is in jail.\nPay $500 to get out?",
                    "Jail",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    if (player.PlayerMoney >= 500)
                    {
                        player.PlayerMoney -= 500;
                        UpdatePlayerMoneyLabel(currPlayer);
                        inJail[currPlayer] = false;
                        jailTurns[currPlayer] = 0;
                        MessageBox.Show($"{player.Name} paid $500 and will be free next turn");
                        NextPlayer();
                        return;
                    }
                    else
                    {
                        MessageBox.Show($"{player.Name} doesn't have enough money");
                    }
                }
            }
            if (!Rolling)
            {
                Rolling = true;
                RollTimer.Start();
                rollTime = 0;
            }
        }

        private void RollTimer_Tick(object sender, EventArgs e)
        {
            if (rollTime >= endTime)
            {
                RollTimer.Stop();
                Rolling = false;
                int[] result = new int[2];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = GameManager.RollDice();
                }
                Dice1.BackgroundImage = listImage[result[0] - 1];
                Dice2.BackgroundImage = listImage[result[1] - 1];
                if (!Rolling)
                {
                    int d1 = result[0];
                    int d2 = result[1];
                    if (inJail[currPlayer])
                    {
                        HandleJailTurn(d1, d2);
                        return;
                    }
                    if (d1 == d2)
                    {
                        Doubles[currPlayer]++;
                        if (Doubles[currPlayer] == 3)
                        {
                            GoToJail(currPlayer);
                            NextPlayer();
                            return;
                        }
                        int steps = d1 + d2;
                        extraTurn = true;
                        MovePlayer(currPlayer, steps);
                        return;
                    }
                    else
                    {
                        Doubles[currPlayer] = 0;
                        extraTurn = false;
                        int steps = d1 + d2;
                        MovePlayer(currPlayer, steps);
                    }
                }
            }
            else
            {
                rollTime += RollTimer.Interval;
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
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                if (i == 0)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(40, 40);
                    pictureBox.Location = new Point(labels[0].Location.X, labels[0].Location.Y - 15);
                    pictureBox.Image = PlayerManager.Players[i].Avt.Image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.BackColor = Color.Transparent;
                    pnl1.Controls.Add(pictureBox);
                    pictureBox.BringToFront();
                    playerPos[i] = 0;
                }
                else if (i == 1)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(40, 40);
                    pictureBox.Location = new Point(labels[0].Location.X + 36 - 1, labels[0].Location.Y - 15);
                    pictureBox.Image = PlayerManager.Players[i].Avt.Image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.BackColor = Color.Transparent;
                    pnl1.Controls.Add(pictureBox);
                    pictureBox.BringToFront();
                    playerPos[i] = 0;
                }
                else if (i == 2)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(40, 40);
                    pictureBox.Location = new Point(labels[0].Location.X, labels[0].Location.Y + 36 - 16);
                    pictureBox.Image = PlayerManager.Players[i].Avt.Image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.BackColor = Color.Transparent;
                    pnl1.Controls.Add(pictureBox);
                    pictureBox.BringToFront();
                    playerPos[i] = 0;
                }
                else
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Name = "Player" + (i + 1).ToString();
                    pictureBox.Size = new Size(40, 40);
                    pictureBox.Location = new Point(labels[0].Location.X + 36 - 1, labels[0].Location.Y + 36 - 16);
                    pictureBox.Image = PlayerManager.Players[i].Avt.Image;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.BackColor = Color.Transparent;
                    pnl1.Controls.Add(pictureBox);
                    pictureBox.BringToFront();
                    playerPos[i] = 0;
                }
            }
        }

        private void MovePlayer(int player, int steps)
        {
            var pcb = this.Controls.Find("Player" + (player + 1), true).FirstOrDefault() as PictureBox;
            if (pcb == null) return;

            int curr = playerPos[player];
            int target = (curr + steps) % labels.Count;

            moveLogic[player] = (pcb, curr, target);

            MoveTimer.Stop();
            MoveTimer.Tick -= MoveTimer_Tick!;
            MoveTimer.Tick += MoveTimer_Tick!;
            MoveTimer.Interval = 400;
            MoveTimer.Start();
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (!moveLogic.ContainsKey(currPlayer)) return;
            var state = moveLogic[currPlayer];
            var pcb = state.Pcb;
            int curr = state.CurrPos;
            int target = state.TargetPos;
            int next = (curr + 1) % labels.Count;
            if (curr + 1 == labels.Count)
            {
                PlayerManager.Players[currPlayer].PlayerMoney += 2000;
                UpdatePlayerMoneyLabel(currPlayer);
            }
            curr = next;
            foreach (Panel pnl in this.Controls.OfType<Panel>())
            {
                if (pnl.Controls.Contains(pcb))
                {
                    pnl.Controls.Remove(pcb);
                    break;
                }
            }
            Panel newPnl = this.Controls.Find("pnl" + (curr + 1), true).FirstOrDefault() as Panel;
            if (newPnl != null)
            {
                newPnl.Controls.Add(pcb);
                pcb.Location = new Point(labels[curr].Location.X, labels[curr].Location.Y - 15);
                pcb.BringToFront();
            }
            if (curr == target)
            {
                MoveTimer.Stop();
                MoveTimer.Tick -= MoveTimer_Tick;
                playerPos[currPlayer] = curr;
                moveLogic.Remove(currPlayer);
                if (curr == 28)
                {
                    extraTurn = false;
                    Doubles[currPlayer] = 0;
                    GoToJail(currPlayer);
                    if (!extraTurn) NextPlayer();
                    else extraTurn = false;
                    return;
                }

                if (curr == 10)
                {
                    bool someoneInJail = inJail.Any(kv => kv.Value == true && kv.Key != currPlayer);
                    if (someoneInJail)
                    {
                        MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} visits jail and pays $200!");
                        PlayerManager.Players[currPlayer].PlayerMoney -= 200;
                        UpdatePlayerMoneyLabel(currPlayer);
                    }
                }
                if (extraTurn)
                {
                    MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} rolled a double and gets extra turn");
                    extraTurn = false;
                }
                else
                {
                    NextPlayer();
                }
            }
            else
            {
                moveLogic[currPlayer] = (pcb, curr, target);
            }
        }
        private void UpdatePlayerMoneyLabel(int player)
        {
            switch (player)
            {
                case 0:
                    lblPlayerMoney1.Text = "$" + PlayerManager.Players[player].PlayerMoney.ToString();
                    break;
                case 1:
                    lblPlayerMoney2.Text = "$" + PlayerManager.Players[player].PlayerMoney.ToString();
                    break;
                case 2:
                    lblPlayerMoney3.Text = "$" + PlayerManager.Players[player].PlayerMoney.ToString();
                    break;
                case 3:
                    lblPlayerMoney4.Text = "$" + PlayerManager.Players[player].PlayerMoney.ToString();
                    break;
            }
        }
        private void GoToJail(int player)
        {
            MessageBox.Show($"{PlayerManager.Players[player].Name} goes to jail!");
            Doubles[player] = 0;
            inJail[player] = true;
            jailTurns[player] = 0;
            MovePlayerToPosition(player, 10);
        }
        private void HandleJailTurn(int d1, int d2)
        {
            var player = currPlayer;
            var p = PlayerManager.Players[player];
            if (d1 == d2)
            {
                MessageBox.Show($"{p.Name} rolled a double and is free!");
                inJail[player] = false;
                jailTurns[player] = 0;
                MovePlayer(player, d1 + d2);
                return;
            }
            jailTurns[player]++;

            if (jailTurns[player] >= 3)
            {
                MessageBox.Show($"{p.Name}  failed 3 times. Must pay $500 to get out");
                if (p.PlayerMoney >= 500)
                {
                    p.PlayerMoney -= 500;
                    UpdatePlayerMoneyLabel(player);
                }
                else
                {
                    MessageBox.Show($"{p.Name} doesn't have enough money");
                }
                inJail[player] = false;
                jailTurns[player] = 0;
                NextPlayer();
            }
            else
            {
                MessageBox.Show($"{p.Name} failed to roll a double ({jailTurns[player]}/3)");
                NextPlayer();
            }
        }

        private void NextPlayer()
        {
            currPlayer = (currPlayer + 1) % PlayerManager.playerCount;
            label1.Text = PlayerManager.Players[currPlayer].Name;
            label1.ForeColor = PlayerManager.Players[currPlayer].Color;
            var pcb = this.Controls.Find("Player" + (currPlayer + 1), true).FirstOrDefault() as PictureBox;
            if (pcb != null)
                pcb.BringToFront();
        }

        private void MovePlayerToPosition(int player, int targetPos)
        {
            var pcb = this.Controls.Find("Player" + (player + 1), true).FirstOrDefault() as PictureBox;
            if (pcb == null) return;

            foreach (Panel pnl in this.Controls.OfType<Panel>())
                if (pnl.Controls.Contains(pcb))
                {
                    pnl.Controls.Remove(pcb);
                    break;
                }

            Panel newPnl = this.Controls.Find("pnl" + (targetPos + 1), true).FirstOrDefault() as Panel;
            if (newPnl != null)
            {
                newPnl.Controls.Add(pcb);
                pcb.Location = new Point(labels[targetPos].Location.X, labels[targetPos].Location.Y - 15);
                pcb.BringToFront();
            }
            playerPos[player] = targetPos;
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

        public static async Task ClientReciever(Monopoly monopoly, TextBox txbCounter)
        {

        }
    }
}
