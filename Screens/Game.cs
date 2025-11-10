using Monopoly.Scripts;
using Monopoly.Scripts.Sockets;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Media;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Monopoly.Screens
{
    public partial class Game : UserControl
    {
        Board board;
        private readonly Action<double>[] _moneyChangedHandlers = new Action<double>[4];
        private Monopoly monopoly;
        private WaveOutEvent bgMusic, moveSound;
        private WaveFileReader bgReader, moveSoundReader;
        List<Bitmap> listImage = new List<Bitmap>()
        {Properties.Resources.Dice1,
        Properties.Resources.Dice2,
        Properties.Resources.Dice3,
        Properties.Resources.Dice4,
        Properties.Resources.Dice5,
        Properties.Resources.Dice6};
        List<Label> labels;

        #region Offline Properties
        Dictionary<int, bool> passedGo = new Dictionary<int, bool>();
        int X1;
        int X2;
        int Y1;
        int Y2;
        int rollTime = 0;
        int endTime;
        bool Rolling = false;
        bool extraTurn = false;
        bool movementInProgress = false;
        int currPlayer = 0;
        Dictionary<int, int> playerPos = new Dictionary<int, int>();
        Dictionary<int, int> Doubles = new Dictionary<int, int>();
        Dictionary<int, bool> inJail = new Dictionary<int, bool>();
        Dictionary<int, int> jailTurns = new Dictionary<int, int>();
        Dictionary<int, (PictureBox Pcb, int CurrPos, int TargetPos)> moveLogic = new Dictionary<int, (PictureBox, int, int)>();
        #endregion

        #region Online Properties
        List<int> turnOrder;
        List<Panel> panels;
        int activePlr = PlayerManager.playerCount;
        private TaskCompletionSource _rollWaiter;
        private TaskCompletionSource _waiter;
        private TaskCompletionSource<bool> _upgradeWaiter;
        private TaskCompletionSource _pickCardWaiter;
        private TaskCompletionSource<int> _sellWaiter;
        #endregion
        public Game(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            labels = new List<Label>{ pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8,
            pos9, pos10, pos11, pos12, pos13, pos14, pos15, pos16, pos17, pos18, pos19,
            pos20, pos21, pos22, pos23, pos24, pos25, pos26, pos27, pos28, pos29, pos30, pos31, pos32, pos33, pos34, pos35, pos36};
            GameLoader();
            InitAudio();
            
            if (!PlayerManager.isOnline)
            {
                for (int i = 0; i < PlayerManager.playerCount; i++)
                {
                    Doubles[i] = 0;
                    inJail[i] = false;
                    jailTurns[i] = 0;
                }
                pcbInfo.Visible = false;
                pcbChat.Visible = false;
                Dice1.Click += RollDice!;
                Dice2.Click += RollDice!;
                RollTimer.Tick += RollTimer_Tick!;
                RollTimer.Interval = 300;
                endTime = 1000;
                X1 = Dice1.Location.X;
                X2 = Dice2.Location.X;
                Y1 = Dice1.Location.Y;
                Y2 = Dice2.Location.Y;


                label1.Text = PlayerManager.Players[currPlayer].Name;
                label1.ForeColor = PlayerManager.Players[currPlayer].Color;
            }
            else
            {
                pcbInfo.Visible = false;
                pcbSave.Visible = false;
                pcbChat.Location = pcbSave.Location;
                Dice1.Enabled = false;
                Dice2.Enabled = false;
                pcbFortune.Enabled = false;
                pcbChance.Enabled = false;
                turnOrder = new List<int>();
                Dice1.Click += DiceClick!;
                Dice2.Click += DiceClick!;
                pcbChance.Click += CardClick!;
                pcbFortune.Click += CardClick!;

                if (PlayerManager.isAdmin)
                {
                    SocketServerManager.OnDataReceived += SocketServerManager_OnDataReceived;
                    SocketServerManager.OnClientDisconnect += SocketServerManager_OnClientDisconnect;
                    RandomizeTurnOrder();
                    label1.Text = PlayerManager.Players[turnOrder[0]].Name;
                    label1.ForeColor = PlayerManager.Players[turnOrder[0]].Color;
                    Task.Run(() => GameLoop());
                }
                else
                {
                    SocketClientManager.OnDataReceived += SocketClientManager_OnDataReceived;
                }
            }
        }
        private void InitAudio()
        {
            bgReader = new WaveFileReader(new MemoryStream(Properties.Resources.BackGroundMusic));
            bgMusic = new WaveOutEvent();
            bgMusic.Init(bgReader);
            bgMusic.Volume = 0.2f;
            bgMusic.PlaybackStopped += (s, e) =>
            {
                bgReader.Position = 0;
                bgMusic.Play();
            };
            bgMusic.Play();
        }
        private void PlaySFX(byte[] soundBytes, int durationMs, float gain = 1.0f)
        {
            Task.Run(async () =>
            {
                using (var ms = new MemoryStream(soundBytes))
                using (var reader = new WaveFileReader(ms))
                using (var output = new WaveOutEvent())
                {
                    var sampleProvider = reader.ToSampleProvider();
                    var volumeProvider = new VolumeSampleProvider(sampleProvider)
                    {
                        Volume = gain
                    };

                    output.Init(volumeProvider);
                    output.Play();
                    await Task.Delay(durationMs);
                }
            });
        }

        private void GameLoader()
        {
            ReadJson();
            LoadPlayerInfo();
            LoadPlayerInBoard();
            panels = new List<Panel>();
            for (int i = 1; i <= 36; i++)
            {
                Panel panel = (Panel)this.Controls.Find("pnl" + i, true).FirstOrDefault()!;
                if (panel != null)
                {
                    panels.Add(panel);
                    panel.Click += PanelClick!;
                }
            }
        }

        private void ReadJson()
        {
            string boardPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Board.json");
            string json = File.ReadAllText(boardPath);
            board = JsonConvert.DeserializeObject<Board>(json)!;
        }

        private void LoadPlayerInfo()
        {
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                var player = PlayerManager.Players[i];

                if (_moneyChangedHandlers[i] != null)
                    player.OnMoneyChanged -= _moneyChangedHandlers[i];

                switch (i)
                {
                    case 0:
                        pcbPlrAvt1.Image = player.Avt.Image;
                        lblPlayerName1.Text = player.Name;
                        lblPlayerMoney1.Text = $"${player.PlayerMoney}";
                        lblPlayerName1.ForeColor = player.Color;
                        pnlPlr1.Visible = true;

                        _moneyChangedHandlers[i] = (newMoney) =>
                        {
                            lblPlayerMoney1.Text = $"${newMoney}";
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;

                    case 1:
                        pcbPlrAvt2.Image = player.Avt.Image;
                        lblPlayerName2.Text = player.Name;
                        lblPlayerMoney2.Text = $"${player.PlayerMoney}";
                        lblPlayerName2.ForeColor = player.Color;
                        pnlPlr2.Visible = true;

                        _moneyChangedHandlers[i] = (newMoney) =>
                        {
                            lblPlayerMoney2.Text = $"${newMoney}";
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;

                    case 2:
                        pcbPlrAvt3.Image = player.Avt.Image;
                        lblPlayerName3.Text = player.Name;
                        lblPlayerMoney3.Text = $"${player.PlayerMoney}";
                        lblPlayerName3.ForeColor = player.Color;
                        pnlPlr3.Visible = true;

                        _moneyChangedHandlers[i] = (newMoney) =>
                        {
                            lblPlayerMoney3.Text = $"${newMoney}";
                            if (PlayerManager.isOnline)
                            {

                            }
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;

                    case 3:
                        pcbPlrAvt4.Image = player.Avt.Image;
                        lblPlayerName4.Text = player.Name;
                        lblPlayerMoney4.Text = $"${player.PlayerMoney}";
                        lblPlayerName4.ForeColor = player.Color;
                        pnlPlr4.Visible = true;

                        _moneyChangedHandlers[i] = (newMoney) =>
                        {
                            lblPlayerMoney4.Text = $"${newMoney}";
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;
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
                    pictureBox.Enabled = false;
                    pictureBox.Name = "Player " + (i + 1).ToString();
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
                    pictureBox.Enabled = false;
                    pictureBox.Name = "Player " + (i + 1).ToString();
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
                    pictureBox.Enabled = false;
                    pictureBox.Name = "Player " + (i + 1).ToString();
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
                    pictureBox.Enabled = false;
                    pictureBox.Name = "Player " + (i + 1).ToString();
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

        #region Offline Methods
        private void RollDice(object sender, EventArgs e)
        {
            if (movementInProgress) return;
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
                        ShowPopup(labels[10], -500);
                        PlaySFX(Properties.Resources.Money, 500);
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
                PlaySFX(Properties.Resources.DiceSound, 1000);
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
                if (rollTime < endTime / 3)
                    RollTimer.Interval = 40;
                else if (rollTime < endTime * 2 / 3)
                    RollTimer.Interval = 70;
                else
                    RollTimer.Interval = 120;
                Random rnd = new Random();
                Dice1.BackgroundImage = listImage[rnd.Next(6)];
                Dice2.BackgroundImage = listImage[rnd.Next(6)];
                int shakeRange = Math.Max(1, 8 - rollTime / 250);
                int offsetX = rnd.Next(-shakeRange, shakeRange + 1);
                int offsetY = rnd.Next(-shakeRange, shakeRange + 1);

                Dice1.Location = new Point(X1 + offsetX, Y1 + offsetY);
                Dice2.Location = new Point(X2 + offsetX, Y2 + offsetY);
            }
        }

        private void MovePlayer(int player, int steps)
        {
            var pcb = this.Controls.Find("Player " + (player + 1), true).FirstOrDefault() as PictureBox;
            if (pcb == null) return;

            int curr = playerPos[player];
            int target = (curr + steps) % labels.Count;
            bool hasPassed = (curr + steps) >= labels.Count;
            passedGo[player] = hasPassed;
            moveLogic[player] = (pcb, curr, target);
            movementInProgress = true;
            Dice1.Enabled = false;
            Dice2.Enabled = false;

            MoveTimer.Stop();
            MoveTimer.Tick -= MoveTimer_Tick!;
            MoveTimer.Tick += MoveTimer_Tick!;
            MoveTimer.Interval = 400;
            MoveTimer.Start();
        }
        private void ShowPopup(Label targetLabel, double amount)
        {
            if (targetLabel == null) return;
            bool isPositive = amount >= 0;
            string sign = isPositive ? "+" : "-";
            string finalText = $"{sign}{Math.Abs(amount)}$";
            Color baseColor = isPositive ? Color.Gold : Color.Red;

            Label popup = new Label
            {
                Text = finalText,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = baseColor,
            };
            var parentPanel = targetLabel.Parent ?? this;
            parentPanel.Controls.Add(popup);
            popup.BringToFront();
            Size pref = popup.PreferredSize;

            int popupX = targetLabel.Left + (targetLabel.Width - pref.Width) / 2;
            int popupY = targetLabel.Top - pref.Height - 4;
            if (popupY < 0)
                popupY = targetLabel.Top + targetLabel.Height + 4;
            popup.Location = new Point(popupX, popupY);
            System.Windows.Forms.Timer popupTimer = new System.Windows.Forms.Timer();
            int steps = 0;
            int maxSteps = 18;
            popupTimer.Interval = 35;
            popupTimer.Tick += (s, e) =>
            {
                steps++;
                popup.Top -= 2;
                int alpha = (int)(255 * (1 - (double)steps / maxSteps));
                alpha = Math.Max(0, Math.Min(255, alpha));
                popup.ForeColor = Color.FromArgb(alpha, baseColor);

                if (steps >= maxSteps)
                {
                    popupTimer.Stop();
                    popupTimer.Dispose();
                    parentPanel.Controls.Remove(popup);
                    popup.Dispose();
                }
            };
            popupTimer.Start();
        }

        private async void MoveTimer_Tick(object sender, EventArgs e)
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
                ShowPopup(labels[0],2000);
                PlaySFX(Properties.Resources.Money, 500);
            }
            curr = next;
            PlaySFX(Properties.Resources.Move, 300, 2F);
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
                MoveTimer.Tick -= MoveTimer_Tick!;
                playerPos[currPlayer] = curr;
                moveLogic.Remove(currPlayer);
                movementInProgress = false;

                if (/*curr == 28*/ true)
                {
                    extraTurn = false;
                    Doubles[currPlayer] = 0;
                    await GoToJail(currPlayer);
                    if (!extraTurn)
                    {
                        NextPlayer();
                        Dice1.Enabled = true;
                        Dice2.Enabled = true;
                    }

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
                        ShowPopup(labels[10], -200);
                        PlaySFX(Properties.Resources.Money, 500);
                    }
                }
                if (curr == 3)
                {
                    double tax = PlayerManager.Players[currPlayer].PlayerMoney * 0.1;
                    PlaySFX(Properties.Resources.Money, 500);
                    PlayerManager.Players[currPlayer].PlayerMoney -= tax;
                    ShowPopup(labels[3], (0-tax));
                    MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} Pay Income Tax -10% Property Value");
                }
                if (curr == 26)
                {
                    PlayerManager.Players[currPlayer].PlayerMoney -= 2000;
                    ShowPopup(labels[26], -2000);
                    PlaySFX(Properties.Resources.Money, 500);
                    MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} Pay Luxury Tax -2000$");
                }
                if (extraTurn)
                {
                    MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} rolled a double and gets extra turn");
                    extraTurn = false;
                    Dice1.Enabled = true;
                    Dice2.Enabled = true;
                }
                else
                {
                    NextPlayer();
                    Dice1.Enabled = true;
                    Dice2.Enabled = true;
                }
            }
            else
            {
                moveLogic[currPlayer] = (pcb, curr, target);
            }
        }
        private async Task GoToJail(int player)
        {
            MessageBox.Show($"{PlayerManager.Players[player].Name} goes to jail!");
            Doubles[player] = 0;
            inJail[player] = true;
            jailTurns[player] = 0;
            await MovePlayerToPosition(player, 10);
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
                    PlaySFX(Properties.Resources.Money, 500);
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
            var pcb = this.Controls.Find("Player " + (currPlayer + 1), true).FirstOrDefault() as PictureBox;
            if (pcb != null)
                pcb.BringToFront();
        }

        private async Task MovePlayerToPosition(int player, int targetPos)
        {
            await this.Invoke(async() => {
                if (movementInProgress) return;
                movementInProgress = true;
                var pcb = this.Controls.Find("Player " + (player + 1), true).FirstOrDefault() as PictureBox;
                if (pcb == null) { movementInProgress = false; return; }
                int startPos = playerPos[player];
                Panel startPanel = this.Controls.Find("pnl" + (startPos + 1), true).FirstOrDefault() as Panel;
                Panel targetPanel = this.Controls.Find("pnl" + (targetPos + 1), true).FirstOrDefault() as Panel;
                if (startPanel == null || targetPanel == null) { movementInProgress = false; return; }
                var portal = new MemoryStream(Properties.Resources.Portal);
                var portalStream = new MemoryStream(Properties.Resources.Portal);
                portalStream.Position = 0;
                PictureBox portalStart = new PictureBox
                {
                    Size = new Size(50, 50),
                    BackColor = Color.Transparent,
                    Image = Image.FromStream(portalStream),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(labels[startPos].Location.X, labels[startPos].Location.Y - 20)
                };

                PictureBox portalEnd = new PictureBox
                {
                    Size = new Size(50, 50),
                    BackColor = Color.Transparent,
                    Image = Image.FromStream(portalStream),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(labels[targetPos].Location.X, labels[targetPos].Location.Y - 20)
                };

                startPanel.Controls.Add(portalStart);
                targetPanel.Controls.Add(portalEnd);
                portalStart.BringToFront();
                portalEnd.BringToFront();
                startPanel.Controls.Remove(pcb);
                portalStart.Controls.Add(pcb);
                pcb.Location = new Point(5, 5);
                pcb.BringToFront();
                PlaySFX(Properties.Resources.Teleport, 400);
                //await Task.Run(() => Task.Delay(200));
                portalStart.Controls.Remove(pcb);
                portalEnd.Controls.Add(pcb);
                pcb.Location = new Point(5, 5);
                //await Task.Run(() =>  Task.Delay(200)); 
                portalEnd.Controls.Remove(pcb);
                targetPanel.Controls.Add(pcb);
                pcb.Location = new Point(labels[targetPos].Location.X, labels[targetPos].Location.Y - 15);
                pcb.BringToFront();
                startPanel.Controls.Remove(portalStart);
                targetPanel.Controls.Remove(portalEnd);
                portalStart.Dispose();
                portalEnd.Dispose();
                playerPos[player] = targetPos;
                movementInProgress = false;
            });
        }

        #endregion
        #region Online Methods
        private void RandomizeTurnOrder()
        {
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                turnOrder.Add(i);
            }
            Random rnd = new Random();

            for (int i = turnOrder.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (turnOrder[i], turnOrder[j]) = (turnOrder[j], turnOrder[i]);
            }
        }
        private int[] RollDiceOnline()
        {
            Random random = new Random();
            int dice1 = random.Next(1, 7);
            int dice2 = random.Next(1, 7);
            return new int[] { dice1, dice2 };
        }

        private async Task GameLoop()
        {
            int steps;
            int currentPlayerIndex = 0;
            while (true)
            {
                _rollWaiter = new TaskCompletionSource();
                int playerIndex = turnOrder[currentPlayerIndex];
                if (PlayerManager.Players[playerIndex].Name == "Player 1")
                {
                    Dice1.Enabled = true;
                    Dice2.Enabled = true;
                }
                await SocketServerManager.Broadcast(new Data(Data.ActionType.RollDice, PlayerManager.Players[playerIndex].Name));

                await _rollWaiter.Task;
                int[] Dices = RollDiceOnline();
                steps = Dices[0] + Dices[1];
                Dice1.Image = listImage[Dices[0] - 1];
                Dice2.Image = listImage[Dices[1] - 1];
                BroadcastDice(PlayerManager.Players[playerIndex].Name, Dices[0], Dices[1]);
                await PlayerMoving(PlayerManager.Players[playerIndex].Name, steps);
                _waiter = new TaskCompletionSource();
                CheckPlrPos(playerIndex);
                await _waiter.Task;
                currentPlayerIndex = (currentPlayerIndex + 1) % turnOrder.Count;
            }
        }
        private async void CheckPlrPos(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var propType = board.properties[plr.Position - 1].type;

            if (propType == "community_chest")
            {
                await DrawCard(plrIndex, "community_chest");
                _waiter?.SetResult();
            }
            else if (propType == "chance")
            {
                await DrawCard(plrIndex, "chance");
                _waiter?.SetResult();
            }
            else if (propType == "Jail")
            {
                GoToJailOnl(plrIndex);
                await SocketServerManager.Broadcast(new Data(Data.ActionType.GoToJail, null, plr.Name));
                _waiter?.SetResult();
            }
            else if (propType == "tax")
            {
                TaxHandler(plrIndex, board.properties[plr.Position - 1].name);
            }
            else
            {
                PropHandler(plrIndex);
            }
        }
        private void TaxHandler(int plrIndex, string name)
        {
            var plr = PlayerManager.Players[plrIndex];
            if (name == "Income Tax")
            {
                plr.PlayerMoney *= 0.9;
            }
            else
            {
                plr.PlayerMoney -= 2000;
            }
        }
        private async Task DrawCard(int plrIndex, string propType)
        {
            Card card;
            var plr = PlayerManager.Players[plrIndex];
            _pickCardWaiter = new TaskCompletionSource();
            if (propType == "community_chest")
            {
                if (plr.Name == "Player 1")
                {
                    pcbFortune.Enabled = true;
                }
                else
                {
                    await SocketServerManager.SocketServer.Send(SocketServerManager.ClientSockets[plr.Name], new Data(Data.ActionType.PickCard, "community_chest"));
                }
                card = board.fortune![new Random().Next(board.chance.Count)];
            }
            else
            {
                if (plr.Name == "Player 1")
                {
                    pcbChance.Enabled = true;
                }
                else
                {
                    await SocketServerManager.SocketServer.Send(SocketServerManager.ClientSockets[plr.Name], new Data(Data.ActionType.PickCard, "chance"));
                }
                card = board.chance![new Random().Next(board.chance.Count)];
            }
            await _pickCardWaiter.Task;
            switch (card.action)
            {
                case CardAction.Receive:
                    plr.PlayerMoney += card.value;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Recieve, $"{card.description}/{plr.PlayerMoney}", plr.Name));
                    _ = Task.Run(() => MessageBox.Show(card.description, plr.Name));
                    break;
                case CardAction.Pay:
                    plr.PlayerMoney -= card.value;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Pay, $"{card.description}/{plr.PlayerMoney}", plr.Name));
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    break;
                case CardAction.PayEachPlayer:
                    plr.PlayerMoney -= card.value * activePlr;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.PayEachPlayer, $"{card.description}/{card.value}/{card.value * activePlr}", plr.Name));
                    foreach (var p in PlayerManager.Players)
                    {
                        if (p == plr || p.IsBankrupt == true) continue;
                        p.PlayerMoney += card.value;
                    }
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    break;
                case CardAction.ReceiveFromEachPlayer:
                    plr.PlayerMoney += card.value * activePlr;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.ReceiveFromEachPlayer, $"{card.description}/{card.value}/{card.value * activePlr}", plr.Name));
                    foreach (var p in PlayerManager.Players)
                    {
                        if (p == plr || p.IsBankrupt == true) continue;
                        p.PlayerMoney -= card.value;
                    }
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    break;
                case CardAction.GetOutOfJailFree:
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.GetOutOfJailFree, card.description, plr.Name));
                    plr.GetOutOfJailFree++;
                    break;
                case CardAction.Jail:
                    GoToJailOnl(plrIndex);
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.GoToJail, null, plr.Name));
                    break;
            }

        }
        private void GoToJailOnl(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var pcb = (PictureBox)this.Controls.Find(plr.Name, true).FirstOrDefault()!;
            this.Invoke(new Action(() =>
            {
                pcb.Location = new Point(labels[10].Location.X, labels[10].Location.Y - 15);
                panels[10].Controls.Add(pcb);
                pcb.BringToFront();
            }));
            plr.IsInJail = true;
        }

        private void PropHandler(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            if (board.properties[plr.Position - 1].owner == null)
            {
                BuyProp(plrIndex);
            }
            else
            {
                if (board.properties[plr.Position - 1].owner == plr.Name)
                {
                    if (board.properties[plr.Position - 1].type == "utility")
                        return;
                    UpdgradeProp(plrIndex);
                }
                else
                {
                    PayRent(plrIndex);
                }
            }
        }
        private void UpdgradeProp(int plrIndex)
        {

        }
        private void BuyProp(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var prop = board.properties[plr.Position - 1];
            var result = MessageBox.Show(
                $"Do you want to buy {prop.name} for ${prop.price}?",
                "Purchase Property",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes)
            {
                if (plr.PlayerMoney > prop.price)
                {

                }
            }
            else
            {

            }
            panels[plrIndex].BackColor = plr.Color;
            _waiter?.SetResult();
        }
        private bool SellProp(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            if (plr.OwnedProperties.Count == 0)
            {
                return false;
            }
            return true;
        }
        private void PayRent(int plrIndex)
        {
        }

        private async void BroadcastDice(string plr, int dice1, int dice2)
        {
            foreach (var client in SocketServerManager.ClientSockets)
            {
                if (client.Key == "Player 1") continue;
                if (client.Value != null)
                {
                    await SocketServerManager.SocketServer.Send(client.Value, new Data(Data.ActionType.Dice, $"{dice1}/{dice2}", plr));
                }
            }
        }

        private async Task PlayerMoving(string plr, int steps)
        {
            var plrInfo = PlayerManager.Players.FirstOrDefault(x => x.Name.Equals(plr));
            if (plrInfo == null) return;

            var pcb = (PictureBox)this.Controls.Find(plr, true).FirstOrDefault()!;
            if (pcb == null) return;

            int oldPos = plrInfo.Position;
            bool passedStart = (plrInfo.Position + steps) >= labels.Count;
            plrInfo.Position = (plrInfo.Position + steps) % labels.Count;

            for (int i = 0; i < steps; i++)
            {
                int pos = (oldPos + i) % labels.Count;
                this.Invoke(new Action(() =>
                {
                    pcb.Location = new Point(labels[pos].Location.X, labels[pos].Location.Y - 15);
                    panels[pos].Controls.Add(pcb);
                    pcb.BringToFront();
                }));
                await Task.Delay(150);
            }

            if (passedStart)
            {
                _ = Task.Run(() => MessageBox.Show($"{plrInfo.Name} passed GO! Collect $2000"));
                if (PlayerManager.isAdmin)
                {
                    plrInfo.PlayerMoney += 2000;

                    foreach (var client in SocketServerManager.ClientSockets)
                    {
                        if (client.Key == "Player 1") continue;
                        if (client.Value != null)
                        {
                            await SocketServerManager.SocketServer.Send(
                                client.Value,
                                new Data(Data.ActionType.Recieve, $"2000/{plrInfo.PlayerMoney}", plr)
                            );
                        }
                    }
                }
            }
        }

        private void SocketServerManager_OnDataReceived(object sender, Data data)
        {
            switch (data.Action)
            {
                case Data.ActionType.RollDice:
                    _rollWaiter?.SetResult();
                    break;
                case Data.ActionType.PickCard:
                    _pickCardWaiter?.SetResult();
                    break;
            }
        }

        private void SocketServerManager_OnClientDisconnect(Socket socket, string arg2)
        {

        }

        private void SocketClientManager_OnDataReceived(Data data)
        {
            switch (data.Action)
            {
                case Data.ActionType.RollDice:
                    this.Invoke(() =>
                    {
                        label1.Text = data.Msg;
                        label1.ForeColor = PlayerManager.Players.FirstOrDefault(x => x.Name == data.Msg)!.Color;

                        var plr = PlayerManager.Players.FirstOrDefault(y => y.Name == data.Msg);
                        if (plr != null)
                        {
                            Dice1.Enabled = true;
                            Dice2.Enabled = true;
                        }
                    });
                    break;
                case Data.ActionType.Dice:
                    this.Invoke(async () =>
                    {
                        string[] Dices = data.Msg!.Split("/");
                        Dice1.Image = listImage[int.Parse(Dices[0]) - 1];
                        Dice2.Image = listImage[int.Parse(Dices[1]) - 1];
                        await PlayerMoving(data.From!, (int.Parse(Dices[0]) + int.Parse(Dices[1])));
                    });
                    break;
                case Data.ActionType.Recieve:
                    {
                        var plrInfo = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        Task.Run(() => MessageBox.Show(values[0], plrInfo!.Name));
                        plrInfo!.PlayerMoney = int.Parse(values[1]);
                        break;
                    }
                case Data.ActionType.Pay:
                    {
                        var plrInfo = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        Task.Run(() => MessageBox.Show(values[0], plrInfo!.Name));
                        plrInfo!.PlayerMoney = int.Parse(values[1]);
                        break;
                    }
                case Data.ActionType.GoToJail:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        int plrIndex = PlayerManager.Players.IndexOf(plr!);
                        GoToJailOnl(plrIndex);
                        if (data.Msg != null) Task.Run(() => MessageBox.Show(data.Msg, plr!.Name));
                        break;
                    }
                case Data.ActionType.GetOutOfJailFree:
                    Task.Run(() => MessageBox.Show(data.Msg, data.From!));
                    break;
                case Data.ActionType.PickCard:
                    this.Invoke(() =>
                    {
                        if (data.Msg == "community_chest")
                        {
                            pcbFortune.Enabled = true;
                        }
                        else
                        {
                            pcbChance.Enabled = true;
                        }
                    });
                    break;
                case Data.ActionType.ReceiveFromEachPlayer:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        Task.Run(() => MessageBox.Show(values[0], plr!.Name));
                        foreach (var p in PlayerManager.Players)
                        {
                            if (p == plr || p.IsBankrupt == true) continue;
                            p.PlayerMoney -= int.Parse(values[1]);
                        }
                        plr!.PlayerMoney += int.Parse(values[2]);
                        break;
                    }
                case Data.ActionType.PayEachPlayer:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        Task.Run(() => MessageBox.Show(values[0], plr!.Name));
                        foreach (var p in PlayerManager.Players)
                        {
                            if (p == plr || p.IsBankrupt == true) continue;
                            p.PlayerMoney += int.Parse(values[1]);
                        }
                        plr!.PlayerMoney -= int.Parse(values[2]);
                        break;
                    }
            }
        }

        private void DiceClick(object sender, EventArgs e)
        {
            Dice1.Enabled = false;
            Dice2.Enabled = false;
            if (PlayerManager.isAdmin)
            {
                _rollWaiter?.SetResult();
            }
            else
            {
                Task.Run(async () => await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.RollDice)));
            }
        }
        #endregion

        private void PanelClick(object sender, EventArgs e)
        {
            var panel = (Panel)sender;
            var name = panel.Name;
            int num = int.Parse(name.Replace("pnl", ""));
            var prop = board.properties[num - 1];
            StringBuilder sb = new StringBuilder();
            sb.Append($"{prop.name}\n");
            sb.Append($"{prop.description}\n");
            if (prop.type == "property" || prop.type == "utility")
            {
                if (prop.type == "property")
                {
                    sb.Append($"======================\n");
                    foreach (var i in prop.levels!)
                    {
                        string msg = $"Level {i.Key}: {i.Value}\n";
                        sb.Append(msg);
                    }
                    sb.Append($"======================\n");
                }
                if (prop.owner != null)
                {
                    sb.Append($"Owner: {prop.owner}\t");
                    sb.Append($"Current Level: {prop.level}\n");
                }
                sb.Append($"Price: {prop.price}\t");
                sb.Append($"Sell Price: {prop.sell_price}\n");
                sb.Append($"Upgrade Price: {prop.upgrade_price + (prop.level * prop.upgrade_price * 0.25)}");
            }
            MessageBox.Show(sb.ToString(), "Info");
        }

        private void pcbReturn_Click(object sender, EventArgs e)
        {
            if (PlayerManager.isOnline)
            {
                if (PlayerManager.isAdmin)
                {
                    SocketServerManager.SocketServer.Stop();
                    SocketServerManager.OnDataReceived -= SocketServerManager_OnDataReceived;
                    SocketServerManager.OnClientDisconnect -= SocketServerManager_OnClientDisconnect;
                }
                else
                {
                    SocketClientManager.SocketClient.Disconnect();
                    SocketClientManager.OnDataReceived -= SocketClientManager_OnDataReceived;
                }
            }
            monopoly.LoadScreen(new MainMenu(monopoly));
        }
        private void CardClick(object sender, EventArgs e)
        {
            pcbChance.Enabled = false;
            pcbFortune.Enabled = false;
            if (!PlayerManager.isAdmin)
            {
                Task.Run(async () => await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.PickCard)));
            }
            else
            {
                _pickCardWaiter?.SetResult();
            }
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
