using Monopoly.Scripts;
using Monopoly.Scripts.Sockets;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Monopoly.Screens
{
    public partial class Game : UserControl
    {
        Board board;
        private readonly Action<double>[] _moneyChangedHandlers = new Action<double>[4];
        private Monopoly monopoly;
        private WaveOutEvent bgMusic;
        private WaveFileReader bgReader;
        List<Bitmap> listImage = new List<Bitmap>()
        {Properties.Resources.Dice1,
        Properties.Resources.Dice2,
        Properties.Resources.Dice3,
        Properties.Resources.Dice4,
        Properties.Resources.Dice5,
        Properties.Resources.Dice6};
        List<Label> labels;

        #region Offline Properties
        private string dbPath;
        List<Panel> horizontal = new List<Panel>();
        List<Panel> vertical = new List<Panel>();
        Dictionary<int, bool> passedGo = new Dictionary<int, bool>();
        List<int> activePlayers;
        int[] result = new int[2];
        int direction;
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
        int unSeenMsg = 0;
        int currentPlayerIndex = 0;
        List<int> turnOrder;
        List<Panel> panels;
        int activePlr = PlayerManager.playerCount;
        private CancellationTokenSource _gameLoopCts;
        private TaskCompletionSource<bool> _rollWaiter;
        private TaskCompletionSource<bool> _upgradeWaiter;
        private TaskCompletionSource<bool> _pickCardWaiter;
        private TaskCompletionSource<bool> _buyWaiter;
        private int currPlayerIndex;
        #endregion
        public Game(Monopoly monopoly, string saveFile = "")
        {
            InitializeComponent();
            this.monopoly = monopoly;

            dbPath = string.IsNullOrEmpty(saveFile)
           ? Path.Combine(Application.StartupPath, "savegame.sqlite")
           : saveFile;
            horizontal = new List<Panel> { pnl2, pnl6, pnl7, pnl8, pnl10, pnl20, pnl22, pnl23, pnl25, pnl26, pnl28 };
            vertical = new List<Panel> { pnl12, pnl13, pnl15, pnl16, pnl18, pnl30, pnl31, pnl33, pnl35, pnl36 };
            labels = new List<Label>{ pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8,
            pos9, pos10, pos11, pos12, pos13, pos14, pos15, pos16, pos17, pos18, pos19,
            pos20, pos21, pos22, pos23, pos24, pos25, pos26, pos27, pos28, pos29, pos30, pos31, pos32, pos33, pos34, pos35, pos36};
            this.monopoly = monopoly;
            GameLoader();
            InitAudio();

            if (!PlayerManager.isOnline)
            {
                if (PlayerManager.IsLoadingSavedGame)
                {
                    LoadGame();
                    PlayerManager.IsLoadingSavedGame = false;
                }
                for (int i = 0; i < PlayerManager.playerCount; i++)
                {
                    Doubles[i] = 0;
                    inJail[i] = false;
                    jailTurns[i] = 0;
                }
                activePlayers = new List<int>();
                for (int i = 0; i < PlayerManager.playerCount; i++)
                {
                    activePlayers.Add(i);
                }
                currPlayer = activePlayers[0];
                pcbInfo.Visible = false;
                pcbChat.Visible = false;
                lblUnread.Visible = false;
                pnlChat.Visible = false;
                txbChat.Visible = false;
                txbChatHistory.Visible = false;
                Dice1.Click += RollDice!;
                Dice2.Click += RollDice!;
                pcbChance.Click += PickChance!;
                pcbFortune.Click += PickFortune!;
                pcbSave.Click += pcbSaveGame!;
                pcbReturn.Click -= pcbReturnMenu;
                pcbReturn.Click += pcbReturnMenu;
                RollTimer.Tick += RollTimer_Tick!;
                RollTimer.Interval = 300;
                endTime = 1000;
                X1 = Dice1.Location.X;
                X2 = Dice2.Location.X;
                Y1 = Dice1.Location.Y;
                Y2 = Dice2.Location.Y;
                pcbFortune.Enabled = false;
                pcbChance.Enabled = false;

                label1.Text = PlayerManager.Players[currPlayer].Name;
                label1.ForeColor = PlayerManager.Players[currPlayer].Color;
            }
            else
            {
                pcbInfo.Visible = false;
                pcbSave.Visible = false;
                pcbChat.Location = pcbSave.Location;
                lblUnread.Location = pcbChat.Location;
                lblUnread.Visible = false;
                Dice1.Enabled = false;
                Dice2.Enabled = false;
                pcbFortune.Enabled = false;
                pcbChance.Enabled = false;
                turnOrder = new List<int>();
                Dice1.Click += DiceClick!;
                Dice2.Click += DiceClick!;
                pcbChance.Click += CardClick!;
                pcbFortune.Click += CardClick!;
                pcbReturn.Click += pcbReturn_Click!;

                if (PlayerManager.isAdmin)
                {
                    SocketServerManager.OnDataReceived += SocketServerManager_OnDataReceived;
                    SocketServerManager.OnClientDisconnect += SocketServerManager_OnClientDisconnect;
                    RandomizeTurnOrder();

                    pnlChat.Visible = false;
                    _gameLoopCts = new CancellationTokenSource();
                    Task.Run(() => GameLoop(_gameLoopCts.Token));
                }
                else
                {
                    SocketClientManager.OnDataReceived += SocketClientManager_OnDataReceived;

                    pnlChat.Visible = false;
                }
            }
        }
        private void InitAudio()
        {
            bgReader = new WaveFileReader(new MemoryStream(Properties.Resources.BackGroundMusic));
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
                var socket = SocketServerManager.ClientSockets.ElementAt(i);

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
                        _moneyChangedHandlers[i] = async (newMoney) =>
                        {
                            this.Invoke(() => lblPlayerMoney1.Text = $"${newMoney}");
                            PlaySFX(Properties.Resources.Money, 500);
                            this.Invoke(() => ShowPopup(labels[player.Position - 1], newMoney - player.oldMoney));
                            if (player.PlayerMoney < 0 && !PlayerManager.isOnline)
                            {
                                CheckBankrupt(i);
                            }
                            else
                            {
                                if (player.PlayerMoney == double.MinValue && !player.isPlaying)
                                {
                                    lblPlayerName1.Text = "Disconnect";
                                    lblPlayerName1.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney1.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[0].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    RemovePropHandler(0);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney == double.MinValue && player.isPlaying && player.IsBankrupt)
                                {
                                    lblPlayerName1.Text = "Bankrupt";
                                    lblPlayerName1.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney1.Text = "0"; 
                                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", player.Name));
                                    var pcb = this.Controls.Find(PlayerManager.Players[0].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    RemovePropHandler(0); 
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney < 0 && PlayerManager.isOnline && PlayerManager.isAdmin)
                                {
                                    await PromptSellPropertiesOnl(i, Math.Abs(player.PlayerMoney));
                                }
                            }
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;

                    case 1:
                        pcbPlrAvt2.Image = player.Avt.Image;
                        lblPlayerName2.Text = player.Name;
                        lblPlayerMoney2.Text = $"${player.PlayerMoney}";
                        lblPlayerName2.ForeColor = player.Color;
                        pnlPlr2.Visible = true;

                        _moneyChangedHandlers[i] = async (newMoney) =>
                        {
                            this.Invoke(() => lblPlayerMoney2.Text = $"${newMoney}");
                            PlaySFX(Properties.Resources.Money, 500);
                            this.Invoke(() => ShowPopup(labels[player.Position - 1], newMoney - player.oldMoney));
                            if (player.PlayerMoney < 0 && !PlayerManager.isOnline)
                            {
                                CheckBankrupt(i);
                            }
                            else
                            {
                                if (player.PlayerMoney == double.MinValue && !player.isPlaying)
                                {
                                    lblPlayerName2.Text = "Disconnect";
                                    lblPlayerName2.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney2.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[1].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    RemovePropHandler(1);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney == double.MinValue && player.isPlaying && player.IsBankrupt)
                                {
                                    lblPlayerName2.Text = "Bankrupt";
                                    lblPlayerName2.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney2.Text = "0"; 
                                    var pcb = this.Controls.Find(PlayerManager.Players[1].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", player.Name));
                                    RemovePropHandler(1);
                                    checkEndGame();

                                }
                                else if (player.PlayerMoney < 0 && PlayerManager.isOnline && PlayerManager.isAdmin)
                                {
                                    await SocketServerManager.SocketServer.Send(socket.Value, new Data(Data.ActionType.SellRequest, player.PlayerMoney.ToString(), "1"));
                                }
                            }
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;

                    case 2:
                        pcbPlrAvt3.Image = player.Avt.Image;
                        lblPlayerName3.Text = player.Name;
                        lblPlayerMoney3.Text = $"${player.PlayerMoney}";
                        lblPlayerName3.ForeColor = player.Color;
                        pnlPlr3.Visible = true;

                        _moneyChangedHandlers[i] = async (newMoney) =>
                        {
                            this.Invoke(() => lblPlayerMoney3.Text = $"${newMoney}");
                            PlaySFX(Properties.Resources.Money, 500);
                            this.Invoke(() => ShowPopup(labels[player.Position - 1], newMoney - player.oldMoney));
                            if (player.PlayerMoney < 0 && !PlayerManager.isOnline)
                            {
                                CheckBankrupt(i);
                            }
                            else
                            {
                                if (player.PlayerMoney == double.MinValue && !player.isPlaying)
                                {
                                    lblPlayerName3.Text = "Disconnect";
                                    lblPlayerName3.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney3.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[2].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    RemovePropHandler(2);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney == double.MinValue && player.isPlaying && player.IsBankrupt)
                                {
                                    lblPlayerName3.Text = "Bankrupt";
                                    lblPlayerName3.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney3.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[2].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", player.Name));
                                    RemovePropHandler(2);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney < 0 && PlayerManager.isOnline && PlayerManager.isAdmin)
                                {
                                    await SocketServerManager.SocketServer.Send(socket.Value, new Data(Data.ActionType.SellRequest, player.PlayerMoney.ToString(), "2"));
                                }
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

                        _moneyChangedHandlers[i] = async (newMoney) =>
                        {
                            this.Invoke(() => lblPlayerMoney4.Text = $"${newMoney}");
                            PlaySFX(Properties.Resources.Money, 500);
                            this.Invoke(() => ShowPopup(labels[player.Position - 1], newMoney - player.oldMoney));
                            if (player.PlayerMoney < 0 && !PlayerManager.isOnline)
                            {
                                CheckBankrupt(i);
                            }
                            else
                            {
                                if (player.PlayerMoney == double.MinValue && !player.isPlaying)
                                {
                                    lblPlayerName4.Text = "Disconnect";
                                    lblPlayerName4.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney4.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[3].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    RemovePropHandler(3);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney == double.MinValue && player.isPlaying && player.IsBankrupt)
                                {
                                    lblPlayerName4.Text = "Bankrupt";
                                    lblPlayerName4.BackColor = Color.FromArgb(136, 8, 8);
                                    lblPlayerMoney4.Text = "0";
                                    var pcb = this.Controls.Find(PlayerManager.Players[3].Name, true).FirstOrDefault() as PictureBox;
                                    pcb!.Visible = false;
                                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", player.Name));
                                    RemovePropHandler(3);
                                    checkEndGame();
                                }
                                else if (player.PlayerMoney < 0 && PlayerManager.isOnline && PlayerManager.isAdmin)
                                {
                                    await SocketServerManager.SocketServer.Send(socket.Value, new Data(Data.ActionType.SellRequest, player.PlayerMoney.ToString(), "3"));
                                }
                            }
                        };
                        player.OnMoneyChanged += _moneyChangedHandlers[i];
                        break;
                }
            }
        }
        private void LoadPlayerInBoard()
        {
            for (int i = 0; i <  PlayerManager.playerCount; i++)
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
                    int pos = playerPos.ContainsKey(i) ? playerPos[i] : 0;
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

        private void pcbSaveGame(object sender, EventArgs e)
        {
            if (movementInProgress) return;
            if (Rolling) return;
            if (!PlayerManager.isOnline)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                dbPath = Path.Combine(Application.StartupPath, $"save_{timestamp}.sqlite");
                SaveGame();
            }
        }

        private void pcbReturnMenu(object sender, EventArgs e)
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

            monopoly.LoadScreen(new MainMenu(monopoly));
        }

        private void InitDatabase()
        {
            bool createFile = !File.Exists(dbPath);
            if (createFile)
                SQLiteConnection.CreateFile(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                con.Open();

                string sqlPlayer = @"
                CREATE TABLE IF NOT EXISTS Players (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT,
                    Money INTEGER,
                    Position INTEGER,
                    InJail INTEGER,
                    JailTurns INTEGER,
                    GetOutOfJailFree INTEGER,
                    Doubles INTEGER,
                    IsActive INTEGER,
                    IsBankrupt INTEGER,
                    Color TEXT,
                    currPlayer INT
                );";
                using (var cmd = new SQLiteCommand(sqlPlayer, con))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmdCheck = new SQLiteCommand("PRAGMA table_info(Players);", con))
                {
                    var reader = cmdCheck.ExecuteReader();
                    bool hasAvatar = false;
                    bool hascurrPlayer = false;
                    while (reader.Read())
                    {
                        string col = reader["name"].ToString();
                        if (col == "Avatar") hasAvatar = true;
                        if (col == "currPlayer") hascurrPlayer = true;
                    }
                    if (!hasAvatar)
                    {
                        using (var cmdAlter = new SQLiteCommand("ALTER TABLE Players ADD COLUMN Avatar TEXT;", con))
                        {
                            cmdAlter.ExecuteNonQuery();
                        }
                    }
                    if (!hascurrPlayer)
                    {
                        using (var cmdAlter = new SQLiteCommand("ALTER TABLE Players ADD COLUMN currPlayer INT;", con))
                        {
                            cmdAlter.ExecuteNonQuery();
                        }
                    }
                }

                string sqlPlayerProp = @"
                CREATE TABLE IF NOT EXISTS PlayerProperties (
                    PlayerId INTEGER,
                    PropertyName TEXT,
                    PRIMARY KEY(PlayerId, PropertyName)
                );";
                using (var cmd = new SQLiteCommand(sqlPlayerProp, con))
                {
                    cmd.ExecuteNonQuery();
                }

                string sqlProperty = @"
                CREATE TABLE IF NOT EXISTS Properties (
                    Name TEXT PRIMARY KEY,
                    Owner TEXT,
                    Houses INTEGER
                );";
                using (var cmd = new SQLiteCommand(sqlProperty, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SaveGame()
        {
            InitDatabase();

            using (var con = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    using (var del = new SQLiteCommand("DELETE FROM Players", con, tran)) del.ExecuteNonQuery();
                    using (var del = new SQLiteCommand("DELETE FROM PlayerProperties", con, tran)) del.ExecuteNonQuery();
                    using (var del = new SQLiteCommand("DELETE FROM Properties", con, tran)) del.ExecuteNonQuery();

                    for (int i = 0; i < PlayerManager.playerCount; i++)
                    {
                        var p = PlayerManager.Players[i];

                        string sql = @"INSERT INTO Players 
                        (Id, Name, Money, Position, InJail, JailTurns, GetOutOfJailFree, Doubles, IsActive, IsBankrupt, Color, Avatar, currPlayer)
                        VALUES (@id,@name,@money,@pos,@injail,@jailturns,@gojf,@doubles,@active,@bankrupt,@color,@avatar, @currPlayer)";

                        using (SQLiteCommand cmd = new SQLiteCommand(sql, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", i);
                            cmd.Parameters.AddWithValue("@name", p.Name ?? "");
                            cmd.Parameters.AddWithValue("@money", p.PlayerMoney);
                            cmd.Parameters.AddWithValue("@pos", p.IsBankrupt ? -1 : (playerPos.ContainsKey(i) ? playerPos[i] : 0));
                            cmd.Parameters.AddWithValue("@injail", inJail.ContainsKey(i) && inJail[i] ? 1 : 0);
                            cmd.Parameters.AddWithValue("@jailturns", jailTurns.ContainsKey(i) ? jailTurns[i] : 0);
                            cmd.Parameters.AddWithValue("@gojf", p.GetOutOfJailFree);
                            cmd.Parameters.AddWithValue("@doubles", Doubles.ContainsKey(i) ? Doubles[i] : 0);
                            cmd.Parameters.AddWithValue("@active", (activePlayers != null && activePlayers.Contains(i)) ? 1 : 0);
                            cmd.Parameters.AddWithValue("@bankrupt", p.IsBankrupt ? 1 : 0);
                            cmd.Parameters.AddWithValue("@color", p.Color == Color.Empty ? "" : ColorTranslator.ToHtml(p.Color));
                            cmd.Parameters.AddWithValue("@currPlayer", currPlayer);
                            using (var ms = new MemoryStream())
                            {
                                p.Avt.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                string avatarBase64 = Convert.ToBase64String(ms.ToArray());
                                cmd.Parameters.AddWithValue("@avatar", avatarBase64);
                            }
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var prop in board.properties.Where(a => a.owner == p.Name))
                        {
                            string sqlProp = "INSERT INTO PlayerProperties (PlayerId, PropertyName) VALUES (@pid,@prop)";
                            using (SQLiteCommand cmdProp = new SQLiteCommand(sqlProp, con, tran))
                            {
                                cmdProp.Parameters.AddWithValue("@pid", i);
                                cmdProp.Parameters.AddWithValue("@prop", prop.name ?? "");
                                cmdProp.ExecuteNonQuery();
                            }
                        }
                    }
                    foreach (var prop in board.properties)
                    {
                        string sql = "INSERT OR REPLACE INTO Properties (Name, Owner, Houses) VALUES (@name,@owner,@houses)";
                        using (SQLiteCommand cmd = new SQLiteCommand(sql, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@name", prop.name ?? "");
                            cmd.Parameters.AddWithValue("@owner", (object?)prop.owner ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@houses", prop.level);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                }
            }

            MessageBox.Show("Game saved successfully!", "Save Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetGameStateBeforeLoad()
        {
            activePlayers = new List<int>();
            playerPos = new Dictionary<int, int>();
            Doubles = new Dictionary<int, int>();
            inJail = new Dictionary<int, bool>();
            jailTurns = new Dictionary<int, int>();
            if (panels != null)
            {
                foreach (var p in panels)
                {
                    p.BackColor = Color.Transparent;
                    var housePanel = p.Controls.OfType<PictureBox>().FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
                    if (housePanel != null) housePanel.Controls.Clear();
                }
            }
        }
        private void LoadGame()
        {
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Save file not found!");
                return;
            }

            using (var con = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                con.Open();
                int savedPlayerCount = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Players", con).ExecuteScalar());
                int currPlayerTurn = 0;
                var turnResult = new SQLiteCommand("SELECT currPlayer FROM Players LIMIT 1", con).ExecuteScalar();
                if (turnResult != null && int.TryParse(turnResult.ToString(), out int turn)) currPlayerTurn = turn;

                ResetGameStateBeforeLoad();

                PlayerManager.playerCount = savedPlayerCount;
                PlayerManager.Players = new List<PlayerInfo>();
                for (int i = 0; i < savedPlayerCount; i++) PlayerManager.Players.Add(new PlayerInfo(i));
                using (var cmd = new SQLiteCommand("SELECT * FROM Players ORDER BY Id", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        if (id < 0 || id >= PlayerManager.Players.Count) continue;

                        var player = PlayerManager.Players[id];
                        player.Name = reader["Name"]?.ToString() ?? $"Player{id + 1}";
                        player.PlayerMoney = Convert.ToInt32(reader["Money"]);

                        try { player.Color = ColorTranslator.FromHtml(reader["Color"]?.ToString() ?? ""); }
                        catch { player.Color = Color.Black; }

                        int pos = Convert.ToInt32(reader["Position"]);
                        player.Position = pos;
                        player.IsBankrupt = Convert.ToInt32(reader["IsBankrupt"]) == 1;

                        inJail[id] = Convert.ToInt32(reader["InJail"]) == 1;
                        jailTurns[id] = Convert.ToInt32(reader["JailTurns"]);
                        Doubles[id] = Convert.ToInt32(reader["Doubles"]);
                        player.IsInJail = inJail[id];
                        player.JailTurn = jailTurns[id];
                        string avatarBase64 = reader["Avatar"]?.ToString() ?? "";
                        player.Avt = LoadAvatarFromBase64(avatarBase64);

                        if (!player.IsBankrupt)
                        {
                            activePlayers.Add(id);
                            if (pos >= 0) playerPos[id] = pos;
                        }
                        else
                        {
                            ResetPlayerProperties(id);
                            SetPlayerBankruptUI(id);
                            activePlayers.Remove(id);
                        }
                    }
                }
                LoadPlayerProperties(con);
                LoadProperties(con);

                currPlayer = currPlayerTurn;

            }
            LoadPlayerInfo();
            foreach (var player in PlayerManager.Players)
            {
                if (player.IsBankrupt) continue;

                foreach (var prop in board.properties.Where(p => p.owner == player.Name))
                {
                    var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(prop.name));
                    var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
                    if (pnl != null)
                    {
                        if (prop.level == 0)
                            pnl.BackColor = player.Color;

                        var housePanel = pnl.Controls.OfType<PictureBox>()
                            .FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
                        int direction = horizontal.Contains(pnl) ? 1 :
                                        vertical.Contains(pnl) ? 2 : 0;
                        DrawHouse(prop, housePanel, direction);
                    }
                }
            }
            pnl1.Controls.OfType<PictureBox>()
                .Where(pb => pb.Name.StartsWith("Player"))
                .ToList()
                .ForEach(pb => pnl1.Controls.Remove(pb));
            moveLogic.Clear();
            foreach (var kvp in playerPos)
            {
                int id = kvp.Key;
                int pos = kvp.Value;
                if (pos < 0 || pos >= labels.Count) continue;

                var targetLabel = labels[pos];
                var parentPanel = targetLabel.Parent as Panel;
                if (parentPanel == null) continue;

                var pcb = new PictureBox
                {
                    Name = $"Player {id + 1}",
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    Image = PlayerManager.Players[id].Avt.Image,
                    Enabled = false,
                    Location = new Point(targetLabel.Location.X, targetLabel.Location.Y - 15),
                    Visible = !PlayerManager.Players[id].IsBankrupt
                };
                parentPanel.Controls.Add(pcb);
                pcb.BringToFront();
                moveLogic[id] = (pcb, pos, pos);
            }
            for (int i = 0; i < PlayerManager.playerCount; i ++)
            {
                PlayerManager.Players[i].Position = playerPos[i]+1;
            }
            movementInProgress = false;
            Rolling = false;
            extraTurn = false;

            Dice1.Enabled = true;
            Dice2.Enabled = true;
            Dice1.Click -= RollDice;
            Dice2.Click -= RollDice;
            Dice1.Click += RollDice;
            Dice2.Click += RollDice;

            MessageBox.Show("Game loaded successfully!", "Load Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private PictureBox LoadAvatarFromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return new PictureBox { Image = Properties.Resources.VuVjp };
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(bytes))
                {
                    return new PictureBox { Image = Image.FromStream(ms) };
                }
            }
            catch { return new PictureBox { Image = Properties.Resources.VuVjp }; }
        }
        private void LoadPlayerProperties(SQLiteConnection con)
        {
            using (var cmd = new SQLiteCommand("SELECT * FROM PlayerProperties", con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int pid = Convert.ToInt32(reader["PlayerId"]);
                    if (pid < 0 || pid >= PlayerManager.Players.Count) continue;

                    string propName = reader["PropertyName"]?.ToString();
                    if (string.IsNullOrWhiteSpace(propName)) continue;

                    var prop = board.properties.FirstOrDefault(p => p.name == propName);
                    if (prop != null)
                    {
                        PlayerManager.Players[pid].OwnedProperties.Add(board.properties.IndexOf(prop));
                        prop.owner = PlayerManager.Players[pid].Name;
                    }
                }
            }
        }
        private void LoadProperties(SQLiteConnection con)
        {
            using (var cmd = new SQLiteCommand("SELECT * FROM Properties", con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["Name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var prop = board.properties.FirstOrDefault(p => p.name == name);
                    if (prop != null)
                    {
                        prop.owner = reader["Owner"] != DBNull.Value ? reader["Owner"]?.ToString() : null;
                        prop.level = reader["Houses"] != DBNull.Value ? Convert.ToInt32(reader["Houses"]) : 0;
                    }
                }
            }
        }

        private PictureBox GetPlayerPictureBox(int index)
        {
            switch (index)
            {
                case 0: return pcbPlrAvt1;
                case 1: return pcbPlrAvt2;
                case 2: return pcbPlrAvt3;
                case 3: return pcbPlrAvt4;
                default: return null;
            }
        }
        private void CheckBankrupt(int currPlayer, double amount = 0)
        {
            if (!activePlayers.Contains(currPlayer)) return;

            var player = PlayerManager.Players[currPlayer];

            if (player.PlayerMoney < 0 && player.OwnedProperties.Count > 0)
            {
                MessageBox.Show($"{player.Name} must sell properties");
                PromptSellProperties(currPlayer);
            }
            if ((player.PlayerMoney <= 0 && player.OwnedProperties.Count == 0) || (amount != 0 && !CanPlayerAfford(currPlayer, amount)))
            {
                player.IsBankrupt = true;
                ResetPlayerProperties(currPlayer);
                SetPlayerBankruptUI(currPlayer);
                activePlayers.Remove(currPlayer);
                if (playerPos.ContainsKey(currPlayer))
                {
                    var pcb = GetPlayerPictureBox(currPlayer);
                    if (pcb != null) pcb.Visible = false;
                    playerPos.Remove(currPlayer);
                }
                if (activePlayers.Count == 1)
                {
                    var result = (MessageBox.Show($"{PlayerManager.Players[activePlayers[0]].Name} wins the game!"));
                    if (result == DialogResult.OK)
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
                        monopoly.LoadScreen(new MainMenu(monopoly));
                    }
                }
                if (currPlayer == this.currPlayer)
                    NextPlayer();
            }
        }
        private void SetPlayerBankruptUI(int index)
        {
            switch (index)
            {
                case 0:
                    lblPlayerMoney1.Text = "Bankrupt";
                    pnlPlr1.Enabled = false;
                    break;
                case 1:
                    lblPlayerMoney2.Text = "Bankrupt";
                    pnlPlr2.Enabled = false;
                    break;
                case 2:
                    lblPlayerMoney3.Text = "Bankrupt";
                    pnlPlr3.Enabled = false;
                    break;
                case 3:
                    lblPlayerMoney4.Text = "Bankrupt";
                    pnlPlr4.Enabled = false;
                    break;
            }
        }
        private void PromptSellProperties(int currPlayer, double requiredAmount = 0)
        {
            var player = PlayerManager.Players[currPlayer];
            using (Form frm = new Form())
            {
                frm.Text = $"{player.Name} - Sell Properties";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Size = new Size(520, 420);
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = frm.MinimizeBox = false;

                ListBox lst = new ListBox()
                {
                    Location = new Point(12, 12),
                    Size = new Size(360, 320),
                    Font = new Font("Cooper", 14),
                    ForeColor = Color.Black
                };
                foreach (int idx in player.OwnedProperties.ToList())
                {
                    var prop = board.properties[idx];
                    string line;
                    if (prop.level > 0)
                        line = $"{prop.name} - Level {prop.level} (Lastest house price: ${prop.upgrade_price * (1 + 0.25 * prop.level) * 0.5})";
                    else
                        line = $"{prop.name} - No houses (Price: ${prop.sell_price})";
                    lst.Items.Add(new ListItemForSell { PropertyIndex = idx, Display = line });
                }
                System.Windows.Forms.Button btnSellHouse = new System.Windows.Forms.Button() { Text = "Sell 1 House", Location = new Point(380, 12), Size = new Size(120, 36) };
                System.Windows.Forms.Button btnSellProperty = new System.Windows.Forms.Button() { Text = "Sell Property", Location = new Point(380, 56), Size = new Size(120, 36) };
                System.Windows.Forms.Button btnClose = new System.Windows.Forms.Button() { Text = "Done", Location = new Point(380, 320), Size = new Size(120, 36) };

                Label lblInfo = new Label() { Location = new Point(12, 340), Size = new Size(480, 36), AutoSize = false };
                Action updateInfo = () =>
                {
                    lblInfo.Text = $"Money: ${player.PlayerMoney}    Required: ${(requiredAmount > 0 ? requiredAmount.ToString() : "0")}";
                };

                frm.Controls.Add(lst);
                frm.Controls.Add(btnSellHouse);
                frm.Controls.Add(btnSellProperty);
                frm.Controls.Add(btnClose);
                frm.Controls.Add(lblInfo);
                updateInfo();

                btnSellHouse.Click += (s, e) =>
                {
                    if (lst.SelectedItem == null) { MessageBox.Show("Choose a property to sell house."); return; }
                    var item = (ListItemForSell)lst.SelectedItem;
                    var prop = board.properties[item.PropertyIndex];
                    if (prop.level <= 0) { MessageBox.Show("This property have no house."); return; }
                    double gain = (double)(prop.upgrade_price * (1 + 0.25 * prop.level) * 0.5);
                    prop.level -= 1;
                    player.PlayerMoney += gain;
                    var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(prop.name));
                    var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
                    var housePanel = pnl.Controls.OfType<PictureBox>().FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
                    if (housePanel != null)
                        housePanel.Controls.Clear();
                    if (prop.level >= 4)
                    {
                        var hotel = new PictureBox()
                        {
                            Size = housePanel.Size,
                            BackgroundImage = direction == 1 ?
                                Properties.Resources.HouseVip1 :
                                Properties.Resources.HouseVip2,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Parent = housePanel,
                            Location = new Point(0, 0)
                        };
                    }
                    else
                    {
                        for (int i = 0; i < prop.level; i++)
                        {
                            var house = new PictureBox()
                            {
                                Size = new Size(20, 20),
                                BackgroundImage = Properties.Resources.house,
                                BackgroundImageLayout = ImageLayout.Stretch,
                                Parent = housePanel,

                                Location = direction == 1
                                    ? new Point(i * 27, (housePanel.Height - 20) / 2)
                                    : new Point((housePanel.Width - 20) / 2, i * 27)
                            };
                        }
                    }
                    lst.Items[lst.SelectedIndex] = new ListItemForSell
                    {
                        PropertyIndex = item.PropertyIndex,
                        Display = prop.level > 0 ? $"{prop.name} - Level {prop.level}" : $"{prop.name} - No houses (Price: ${prop.sell_price})"
                    };

                    updateInfo();
                };
                btnSellProperty.Click += (s, e) =>
                {
                    if (lst.SelectedItem == null) { MessageBox.Show("Choose a property to sell."); return; }
                    var item = (ListItemForSell)lst.SelectedItem;
                    var prop = board.properties[item.PropertyIndex];
                    if (prop.level > 0)
                    {
                        var res = MessageBox.Show("You have to sell all house before sell property"); return;
                    }
                    double gain = (double)prop.sell_price;
                    prop.owner = null;
                    prop.level = 0;
                    player.PlayerMoney += gain;
                    player.OwnedProperties.Remove(item.PropertyIndex);
                    var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(prop.name));
                    var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
                    if (pnl != null)
                    {
                        pnl.BackColor = Color.Transparent;
                    }
                    lst.Items.RemoveAt(lst.SelectedIndex);
                    updateInfo();
                };

                btnClose.Click += (s, e) =>
                {
                    frm.DialogResult = DialogResult.OK;
                    frm.Close();
                };

                frm.ShowDialog();
            }
        }
        private class ListItemForSell
        {
            public int PropertyIndex { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }

        private async Task HandlePropertyAsync(int currPlayer, string propertyName)
        {
            var player = PlayerManager.Players[currPlayer];
            var property = board.properties.FirstOrDefault(p => p.name == propertyName);
            if (property == null) return;
            if (property.type != "property" && property.type != "utility" && property.type != "railroad") return;
            var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(property.name));
            var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
            var pcbHouse = pnl?.Controls.OfType<PictureBox>().FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
            int direction = horizontal.Contains(pnl) ? 1 : vertical.Contains(pnl) ? 2 : 0;
            if (property.owner != null && property.owner != player.Name)
            {
                HandleRent(currPlayer, property);
                return;
            }
            if (property.owner == player.Name)
            {
                if (property.type == "utility" || property.type == "railroad") return;
                HandleUpgrade(currPlayer, property, pcbHouse, lbl, direction);
                return;
            }
            HandleBuyProperty(currPlayer, property, pnl, lbl);
        }
        private void HandleRent(int currplayer, Property property)
        {
            var player = PlayerManager.Players[currPlayer];
            if (player.IsBankrupt || player.IsInJail) return;

            double rent = 0;

            if (property.type == "utility")
            {
                var ownerProps = board.properties.Where(p => p.owner == property.owner && p.type == "utility").Count();
                double roll = result[0] + result[1];

                rent = ownerProps switch
                {
                    1 => roll * 70,
                    2 => roll * 200,
                    _ => 0
                };
            }
            else if (property.type == "railroad")
            {
                rent = 1000;
            }
            else
            {
                rent = (double)property.levels[property.level.ToString()];
            }

            EnsurePlayerCanPay(PlayerManager.Players.IndexOf(player), rent);
            PlayerManager.Players.First(p => p.Name == property.owner).PlayerMoney += rent;
        }
        private void HandleBuyProperty(int currPlayer, Property property, Panel pnl, Label lbl)
        {
            var player = PlayerManager.Players[currPlayer];
            double price = (double)property.price;

            if (!CanPlayerAfford(currPlayer, price))
                return;

            var dr = MessageBox.Show($"{player.Name}, do you want to buy {property.name} for ${price}?",
                                     "Buy Property",
                                     MessageBoxButtons.YesNo);

            if (dr != DialogResult.Yes)
                return;

            TryPayWithSelling(currPlayer, price);
            if (PlayerManager.Players[currPlayer].PlayerMoney < price) return;
            player.PlayerMoney -= price;
            property.owner = player.Name;
            player.OwnedProperties.Add(board.properties.IndexOf(property));

            if (pnl != null)
                pnl.BackColor = player.Color;
        }
        private bool CanPlayerAfford(int currPlayer, double price)
        {
            var player = PlayerManager.Players[currPlayer];
            if (player.PlayerMoney >= price)
                return true;

            double total = 0;

            foreach (var prop in board.properties.Where(p => p.owner == player.Name))
            {
                double sellValue = (double)prop.sell_price;
                for (int lvl = 1; lvl <= prop.level; lvl++)
                {
                    sellValue += (double)((prop.upgrade_price * (1 + 0.25 * lvl)*0.5));
                }

                total += sellValue;
            }

            return (player.PlayerMoney + total) >= price;
        }

        private void TryPayWithSelling(int currPlayer, double price)
        {
            var player = PlayerManager.Players[currPlayer];
            if (player.PlayerMoney >= price) return;

            if (player.OwnedProperties.Count == 0)
                return;

            var sell = MessageBox.Show($"{player.Name} doesn't have enough money. Sell assets?",
                                       "Not enough money",
                                       MessageBoxButtons.YesNo);

            if (sell == DialogResult.Yes)
                PromptSellProperties(PlayerManager.Players.IndexOf(player), price - player.PlayerMoney);
        }
        private void HandleUpgrade(int currPlayer, Property property, PictureBox pcbHouse, Label lbl, int direction)
        {
           
            var player = PlayerManager.Players[currPlayer];
            if (property.level >= 4) return;

            double upgradeCost = (double)(property.upgrade_price * (1 + 0.25 * property.level));
            if (!CanPlayerAfford(currPlayer, upgradeCost))
            return;

            var ask = MessageBox.Show($"{player.Name}, buy house level {property.level + 1} for ${upgradeCost}?",
                                      "Buy House",
                                      MessageBoxButtons.YesNo);

            if (ask != DialogResult.Yes)
                return;

            TryPayWithSelling(currPlayer, upgradeCost);

            player.PlayerMoney -= upgradeCost;
            property.level++;

            DrawHouse(property, pcbHouse, direction);
        }
        private void DrawHouse(Property property, PictureBox pcbHouse, int direction)
        {
            if (pcbHouse == null) return;

            pcbHouse.Controls.Clear();

            if (property.level >= 4)
            {
                var hotel = new PictureBox()
                {
                    Size = pcbHouse.Size,
                    BackgroundImage = direction == 1 ? Properties.Resources.HouseVip1 : Properties.Resources.HouseVip2,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Parent = pcbHouse,
                    Location = new Point(0, 0)
                };
                return;
            }

            for (int i = 0; i < property.level; i++)
            {
                var house = new PictureBox()
                {
                    Size = new Size(20, 20),
                    BackgroundImage = Properties.Resources.house,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Parent = pcbHouse,
                    Location = direction == 1 ?
                        new Point(i * 27, (pcbHouse.Height - 20) / 2) :
                        new Point((pcbHouse.Width - 20) / 2, i * 27)
                };
            }
        }

        private void ResetPlayerProperties(int currPlayer)
        {
            var player = PlayerManager.Players[currPlayer];
            foreach (int propIndex in player.OwnedProperties)
            {
                var prop = board.properties[propIndex];
                prop.owner = null;
                prop.level = 0;
                var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(prop.name));
                var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
                if (pnl != null)
                {
                    pnl.BackColor = Color.Transparent;
                    var housePanel = pnl.Controls.OfType<PictureBox>().FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
                    if (housePanel != null)
                        housePanel.Controls.Clear();
                }
            }

            player.OwnedProperties.Clear();
        }

        private void RollDice(object sender, EventArgs e)
        {
            if (movementInProgress) return;

            var player = PlayerManager.Players[currPlayer];

            if (inJail[currPlayer])
            {
                if (player.GetOutOfJailFree > 0)
                {
                    var useCard = MessageBox.Show(
                        $"{player.Name} has 'Get Out of Jail Free'. Use it?",
                        "Use Get Out Of Jail Free?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (useCard == DialogResult.Yes)
                    {
                        player.GetOutOfJailFree--;
                        inJail[currPlayer] = false;
                        jailTurns[currPlayer] = 0;
                        MessageBox.Show($"{player.Name} used Get Out of Jail Free and will be free next turn");
                        NextPlayer();
                        return;
                    }
                }
                if (!Rolling)
                {
                    DialogResult dr = MessageBox.Show(
                        $"{player.Name} is in jail.\nPay $1000 to get out?",
                        "Jail",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        if (player.PlayerMoney >= 1000)
                        {
                            player.PlayerMoney -= 1000;
                            inJail[currPlayer] = false;
                            jailTurns[currPlayer] = 0;
                            MessageBox.Show($"{player.Name} paid $1000 and will be free next turn");
                            NextPlayer();
                            return;
                        }
                        else
                        {
                            MessageBox.Show($"{player.Name} doesn't have enough money");
                        }
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
            MoveTimer.Interval = 200;
            MoveTimer.Start();
        }
        private async void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (!activePlayers.Contains(currPlayer)) return;
            var state = moveLogic[currPlayer];
            var pcb = state.Pcb;
            int curr = state.CurrPos;
            int target = state.TargetPos;
            int next = (curr + 1) % labels.Count;
            if (curr + 1 == labels.Count)
            {
                PlayerManager.Players[currPlayer].PlayerMoney += 2000;
            }
            curr = next;
            PlaySFX(Properties.Resources.Move, 300, 4);
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
                PlayerManager.Players[currPlayer].Position = curr+1;
                MoveTimer.Stop();
                MoveTimer.Tick -= MoveTimer_Tick!;
                playerPos[currPlayer] = curr;
                moveLogic.Remove(currPlayer);
                movementInProgress = false;
                var property = board.properties.FirstOrDefault(p => labels[curr].Text.TrimStart().StartsWith(p.name));
                if (property != null)
                {
                    await HandlePropertyAsync(currPlayer, property.name);
                }
                if (curr == 2 || curr == 13)
                {
                    ShowPointingGif(pcbChance, true);
                    await HandleCardAsync(pcbChance, board.chance, "Chance");
                    ShowPointingGif(pcbChance, false); 
                }
                else if (curr == 20 || curr == 31)
                {
                    ShowPointingGif(pcbFortune, true, false);
                    await HandleCardAsync(pcbFortune, board.fortune, "Community Chest");
                    ShowPointingGif(pcbFortune, false);
                }

                if (curr == 28)
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
                        MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} visits jail and pays $500!");
                        EnsurePlayerCanPay(currPlayer, 500);
                    }
                }
                if (curr == 3)
                {
                    double tax = PlayerManager.Players[currPlayer].PlayerMoney * 0.1;
                    EnsurePlayerCanPay(currPlayer, tax);
                    MessageBox.Show($"{PlayerManager.Players[currPlayer].Name} Pay Income Tax -10% Property Value");
                }
                if (curr == 26)
                {
                    EnsurePlayerCanPay(currPlayer, 2000);
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
        private void EnsurePlayerCanPay(int currPlayer, double amount)
        {
            var player = PlayerManager.Players[currPlayer];
            if (!CanPlayerAfford(currPlayer, amount))
            {
                CheckBankrupt(currPlayer);
                return;
            }
            while (player.PlayerMoney < amount && player.OwnedProperties.Count > 0)
            {
                MessageBox.Show($"{player.Name} doesn't have enough money to pay ${amount}. Must sell property to continue.");
                PromptSellProperties(currPlayer, amount - player.PlayerMoney);
                if (player.IsBankrupt)
                {
                    CheckBankrupt(currPlayer);
                    return;
                }
            }
            if (player.PlayerMoney >= amount)
            {
                player.PlayerMoney -= amount;
            }
            else
            {
                MessageBox.Show($"{player.Name} cannot afford the payment and is bankrupt!");
                player.IsBankrupt = true;
                player.PlayerMoney = -1;
                CheckBankrupt(currPlayer);
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
                if (p.GetOutOfJailFree > 0)
                {
                    var useCard = MessageBox.Show(
                        $"{p.Name} failed 3 times but has 'Get Out of Jail Free'. Use it to get out?",
                        "Use Get Out Of Jail Free?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (useCard == DialogResult.Yes)
                    {
                        p.GetOutOfJailFree--;
                        inJail[player] = false;
                        jailTurns[player] = 0;
                        MessageBox.Show($"{p.Name} used the card and is free.");
                        NextPlayer();
                        return;
                    }
                }

                MessageBox.Show($"{p.Name} failed 3 times. Must pay $1000 to get out");
                EnsurePlayerCanPay(player, 1000);
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
            if (activePlayers.Count == 0) return;

            int index = activePlayers.IndexOf(currPlayer);
            if (index == -1 || index + 1 >= activePlayers.Count)
            {
                currPlayer = activePlayers[0];
            }
            else
            {
                currPlayer = activePlayers[index + 1];
            }

            label1.Text = PlayerManager.Players[currPlayer].Name;
            label1.ForeColor = PlayerManager.Players[currPlayer].Color;
        }

        private async void PickChance(object sender, EventArgs e)
        {
            if (movementInProgress) return;
            pcbChance.Enabled = false;
            var card = board.chance[new Random().Next(board.chance.Count)];
            var plr = PlayerManager.Players[currPlayer];
            await ShowCardPanelAsync(card, currPlayer, "Chance");
            UpdateCardButtonsState();
        }
        private async void PickFortune(object sender, EventArgs e)
        {
            if (movementInProgress) return;
            pcbFortune.Enabled = false;
            var card = board.fortune[new Random().Next(board.fortune.Count)];
            var plr = PlayerManager.Players[currPlayer];
            await ShowCardPanelAsync(card, currPlayer, "Community Chest");
            UpdateCardButtonsState();
        }
        private async Task HandleCardAsync(PictureBox pcbCard, List<Card> cards, string pileName)
        {
            if (movementInProgress) return;
            pcbCard.Enabled = true;
            bool cardOpened = false;
            var tcs = new TaskCompletionSource<bool>();
            void ClickHandler(object s, EventArgs e)
            {
                if (!cardOpened)
                {
                    cardOpened = true;
                    tcs.TrySetResult(true);
                }
            }
            pcbCard.Click += ClickHandler;
            await tcs.Task;

            pcbCard.Click -= ClickHandler;
            pcbCard.Enabled = false;

            var card = cards[new Random().Next(cards.Count)];
            await ShowCardPanelAsync(card, currPlayer, pileName);
        }
        private bool cardPanelOpen = false;
        private async Task ShowCardPanelAsync(Card card, int currPlayer, string pileName)
        {
            if (cardPanelOpen) return;
            cardPanelOpen = true;
            var tcs = new TaskCompletionSource<bool>();

            var plr = PlayerManager.Players[currPlayer];

            Panel cardPanel = new Panel
            {
                Size = new Size(420, 260),
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            cardPanel.Location = new Point(
                (pnlGame.Width - 420) / 2,
                (pnlGame.Height - 260) / 2
            );

            Label lblTitle = new Label
            {
                Text = $"{pileName} Card",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };
            cardPanel.Controls.Add(lblTitle);

            System.Windows.Forms.TextBox txtDesc = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Text = card.description,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(240, 240, 240),
                Location = new Point(12, 56),
                Size = new Size(396, 120)
            };
            cardPanel.Controls.Add(txtDesc);
            System.Windows.Forms.Button btnOk = new System.Windows.Forms.Button
            {
                Text = "OK",
                Size = new Size(100, 34),
                Location = new Point(160, 210)
            };
            cardPanel.Controls.Add(btnOk);
            pnlGame.Controls.Add(cardPanel);
            cardPanel.BringToFront();
            EventHandler handler = null;
            handler = async (s, e) =>
            {
                btnOk.Click -= handler;
                switch (card.action)
                {
                    case CardAction.Receive:
                        plr.PlayerMoney += card.value;
                        break;
                    case CardAction.Pay:
                        EnsurePlayerCanPay(currPlayer, card.value);
                        break;
                    case CardAction.PayEachPlayer:
                        int others = PlayerManager.playerCount - 1;
                        EnsurePlayerCanPay(currPlayer, card.value * others);
                        foreach (var p in PlayerManager.Players)
                            if (p != plr && !p.IsBankrupt)
                                p.PlayerMoney += card.value;
                        break;
                    case CardAction.ReceiveFromEachPlayer:
                        int others2 = PlayerManager.playerCount - 1;
                        plr.PlayerMoney += card.value * others2;
                        foreach (var p in PlayerManager.Players)
                            if (p != plr && !p.IsBankrupt)
                                EnsurePlayerCanPay(currPlayer, card.value);
                        break;
                    case CardAction.GetOutOfJailFree:
                        plr.GetOutOfJailFree++;
                        break;
                    case CardAction.Jail:
                        await GoToJail(currPlayer);
                        break;
                }

                pnlGame.Controls.Remove(cardPanel);
                cardPanel.Dispose();
                cardPanelOpen = false;
                tcs.SetResult(true);
            };
            btnOk.Click += handler;
            pnlGame.Controls.Add(cardPanel);
            cardPanel.BringToFront();
            await tcs.Task;
        }
        private PictureBox pbPointing;

        private void ShowPointingGif(PictureBox target, bool show, bool isTop = true)
        {
            if (show)
            {
                if (pbPointing != null)
                {
                    pnlGame.Controls.Remove(pbPointing);
                    pbPointing.Dispose();
                    pbPointing = null;
                }

                pbPointing = new PictureBox()
                {
                    Image = isTop ? Properties.Resources.PointingChance : Properties.Resources.PointingFortune,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    Size = new Size(100, 100)
                };
                int x = target.Left + (target.Width - pbPointing.Width) / 2;
                int y = isTop ? target.Top - pbPointing.Height - 5 : target.Bottom + 5;

                pbPointing.Location = new Point(x, y);
                pnlGame.Controls.Add(pbPointing);
                pbPointing.BringToFront();
                pnlGame.Refresh();
            }
            else
            {
                if (pbPointing != null)
                {
                    pnlGame.Controls.Remove(pbPointing);
                    pbPointing.Dispose();
                    pbPointing = null;
                }
            }
        }


        private void UpdateCardButtonsState()
        {
            int pos = playerPos[currPlayer];

            bool isChance = (pos == 2 || pos == 13);
            bool isFortune = (pos == 20 || pos == 31);

            pcbChance.Enabled = isChance;
            pcbFortune.Enabled = isFortune;
            
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

        private async Task MovePlayerToPosition(int player, int targetPos)
        {
            if (movementInProgress) return;
            movementInProgress = true;
            var pcb = this.Controls.Find("Player " + (player + 1), true).FirstOrDefault() as PictureBox;
            if (pcb == null) { movementInProgress = false; return; }
            int startPos = playerPos[player];
            Panel startPanel = this.Controls.Find("pnl" + (startPos + 1), true).FirstOrDefault() as Panel;
            Panel targetPanel = this.Controls.Find("pnl" + (targetPos + 1), true).FirstOrDefault() as Panel;
            if (startPanel == null || targetPanel == null) { movementInProgress = false; return; }
            PictureBox portalStart = null;
            PictureBox portalEnd = null;
            this.Invoke(() =>
            {
                var portalStream = new MemoryStream(Properties.Resources.Portal);
                portalStart = new PictureBox
                {
                    Size = new Size(50, 50),
                    BackColor = Color.Transparent,
                    Image = Image.FromStream(portalStream),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(labels[startPos].Location.X, labels[startPos].Location.Y - 20)
                };
                portalEnd = new PictureBox
                {
                    Size = new Size(50, 50),
                    BackColor = Color.Transparent,
                    Image = Image.FromStream(new MemoryStream(Properties.Resources.Portal)),
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
            });
            PlaySFX(Properties.Resources.Teleport, 400, 5);
            await Task.Delay(200);
            this.Invoke(() =>
            {
                portalStart.Controls.Remove(pcb);
                portalEnd.Controls.Add(pcb);
                pcb.Location = new Point(5, 5);
            });
            await Task.Delay(200);
            this.Invoke(() =>
            {
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
                PlayerManager.Players[currPlayer].Position = targetPos + 1;
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

        private async Task GameLoop(CancellationToken token)
        {
            int steps;
            while (!token.IsCancellationRequested)
            {
                activePlr = 0;
                foreach (var plr in PlayerManager.Players)
                {
                    if (!plr.IsBankrupt && plr.isPlaying)
                    {
                        activePlr++;
                    }
                }
                await Task.Delay(1000);
                if (activePlr == 1)
                {
                    var player = PlayerManager.Players.FirstOrDefault(x => !x.IsBankrupt && x.isPlaying);
                    _ = Task.Run(()=>MessageBox.Show($"{player!.Name} won the game!"));
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.message, $"{player!.Name} won the game!"));
                    break;
                }
                int playerIndex = turnOrder[currentPlayerIndex];
                currPlayerIndex = turnOrder[currentPlayerIndex];
                var currentPlr = PlayerManager.Players[playerIndex];
                if (currentPlr.IsBankrupt || !currentPlr.isPlaying)
                {
                    currentPlayerIndex = (currentPlayerIndex + 1) % turnOrder.Count;
                    continue;
                }
                _rollWaiter = new TaskCompletionSource<bool>();
                if (PlayerManager.Players[playerIndex].Name == "Player 1")
                {
                    Dice1.Enabled = true;
                    Dice2.Enabled = true;
                }
                label1.Text = PlayerManager.Players[playerIndex].Name;
                label1.ForeColor = PlayerManager.Players[playerIndex].Color;
                if (currentPlr.IsInJail)
                {
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.RollDice, PlayerManager.Players[playerIndex].Name, "Roll double to get out of jail"));
                }
                else
                {
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.RollDice, PlayerManager.Players[playerIndex].Name));
                }
                bool checker = await _rollWaiter.Task;
                if (!checker) { continue; }
                int[] Dices = RollDiceOnline();
                steps = Dices[0] + Dices[1];
                Dice1.Image = listImage[Dices[0] - 1];
                Dice2.Image = listImage[Dices[1] - 1];
                if(currentPlr.IsInJail)
                {
                    if(steps%2 == 0)
                    {
                        await SocketServerManager.Broadcast(new Data(Data.ActionType.message,$"{currentPlr.Name} now is free!"));
                        currentPlr.IsInJail = false;
                    }
                    currentPlayerIndex = (currentPlayerIndex + 1) % turnOrder.Count;
                    continue;
                }
                BroadcastDice(PlayerManager.Players[playerIndex].Name, Dices[0], Dices[1]);
                await PlayerMoving(PlayerManager.Players[playerIndex].Name, steps);
                await CheckPlrPos(playerIndex);
                for (int i = 0; i < PlayerManager.playerCount; i++)
                {
                    if (PlayerManager.Players[i].PlayerMoney < 0 && PlayerManager.Players[i].isPlaying)
                    {
                        i--;
                        await Task.Delay(500);
                    }
                }
                if (Dices[0] == Dices[1])
                {
                    PlayerManager.Players[currentPlayerIndex].doubleRollCount++;
                    continue;
                }
                currentPlayerIndex = (currentPlayerIndex + 1) % turnOrder.Count;
            }
        }
        private async Task CheckPlrPos(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var propType = board.properties[plr.Position - 1].type;
            if (propType == "corner") return;
            if (propType == "community_chest")
            {
                await DrawCard(plrIndex, "community_chest");
            }
            else if (propType == "chance")
            {
                await DrawCard(plrIndex, "chance");
            }
            else if (propType == "Jail")
            {
                GoToJailOnl(plrIndex);
                await SocketServerManager.Broadcast(new Data(Data.ActionType.GoToJail, null, plr.Name));
            }
            else if (propType == "Visiting Jail")
            {
                foreach (var player in PlayerManager.Players)
                {
                    if (player.IsInJail == true)
                    {
                        plr.PlayerMoney -= 500;
                        _ = Task.Run(() => MessageBox.Show("Pay $500 for visiting the jail.", plr.Name));
                        await SocketServerManager.Broadcast(new Data(Data.ActionType.Pay, $"Pay $500 for visiting the jail./{plr.PlayerMoney}", plr.Name));
                    }
                }
            }
            else if (propType == "Free Parking")
                return;
            else if (propType == "tax")
            {
                await TaxHandler(plrIndex, board.properties[plr.Position - 1].name);
            }
            else
            {
                await PropHandler(plrIndex);
            }
        }
        private async Task TaxHandler(int plrIndex, string name)
        {
            var plr = PlayerManager.Players[plrIndex];
            if (name == "Income Tax")
            {
                plr.PlayerMoney *= 0.9;
                _ = Task.Run(() => MessageBox.Show("Pay income tax -10% of property value.", plr.Name));
                await SocketServerManager.Broadcast(new Data(Data.ActionType.Pay, $"Pay income tax -10% of property value./{plr.PlayerMoney}", plr.Name));
            }
            else
            {
                plr.PlayerMoney -= 2000;
                _ = Task.Run(() => MessageBox.Show("Pay 2000 luxury tax.", plr.Name));
                await SocketServerManager.Broadcast(new Data(Data.ActionType.Pay, $"Pay 2000 luxury tax./{plr.PlayerMoney}", plr.Name));
            }
        }
        private async Task DrawCard(int plrIndex, string propType)
        {
            Card card;
            var plr = PlayerManager.Players[plrIndex];
            _pickCardWaiter = new TaskCompletionSource<bool>();
            if (propType == "community_chest")
            {
                if (plr.Name == "Player 1")
                {
                    MessageBox.Show("You landed on Fortune! Please draw a card.");
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
                    MessageBox.Show("You landed on Chance! Please draw a card.");
                    pcbChance.Enabled = true;
                }
                else
                {
                    await SocketServerManager.SocketServer.Send(SocketServerManager.ClientSockets[plr.Name], new Data(Data.ActionType.PickCard, "chance"));
                }
                card = board.chance![new Random().Next(board.chance.Count)];
            }
            var checker = await _pickCardWaiter.Task;
            if(checker ==false) return;
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
                        if (p == plr || p.IsBankrupt == true || !p.isPlaying) continue;
                        p.PlayerMoney += card.value;
                    }
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    break;
                case CardAction.ReceiveFromEachPlayer:
                    plr.PlayerMoney += card.value * activePlr;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.ReceiveFromEachPlayer, $"{card.description}/{card.value}/{card.value * activePlr}", plr.Name));
                    foreach (var p in PlayerManager.Players)
                    {
                        if (p == plr || p.IsBankrupt == true || !p.isPlaying) continue;
                        p.PlayerMoney -= card.value;
                    }
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    break;
                case CardAction.GetOutOfJailFree:
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.GetOutOfJailFree, card.description, plr.Name));
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    plr.GetOutOfJailFree++;
                    break;
                case CardAction.Jail:
                    GoToJailOnl(plrIndex);
                    _ = Task.Run(() => MessageBox.Show(card.description));
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.GoToJail, null, plr.Name));
                    break;
            }

        }
        private void GoToJailOnl(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            plr.Position = 11;
            var pcb = (PictureBox)this.Controls.Find(plr.Name, true).FirstOrDefault()!;
            this.Invoke(new Action(() =>
            {
                pcb.Location = new Point(labels[10].Location.X, labels[10].Location.Y - 15);
                panels[10].Controls.Add(pcb);
                pcb.BringToFront();
            }));
            plr.IsInJail = true;
        }

        private async Task PropHandler(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            if (board.properties[plr.Position - 1].owner == null)
            {
                await BuyProp(plrIndex);
            }
            else
            {
                if (board.properties[plr.Position - 1].owner == plr.Name)
                {
                    if (board.properties[plr.Position - 1].type == "utility")
                        return;
                    await UpdgradeProp(plrIndex, plr.Position - 1);
                }
                else
                {
                    await PayRent(plrIndex);
                }
            }
        }
        private void UpdatePropertyVisual(string propertyName, int level)
        {
            this.Invoke(() =>
            {
                var property = board.properties.FirstOrDefault(p => p.name == propertyName);
                if (property == null) return;
                if (property.type != "property") return;

                var lbl = labels.FirstOrDefault(l => l.Text.TrimStart().StartsWith(property.name));
                var pnl = panels.FirstOrDefault(p => p.Controls.Contains(lbl));
                if (pnl == null) return;

                int direction = horizontal.Contains(pnl) ? 1 : 2;

                var pcbHouse = pnl.Controls.OfType<PictureBox>().FirstOrDefault(pb => !pb.Name.StartsWith("Player"));
                if (pcbHouse == null) return;

                pcbHouse.Controls.Clear();

                if (level >= 4)
                {
                    var hotel = new PictureBox()
                    {
                        Size = pcbHouse.Size,
                        BackgroundImage = direction == 1
                            ? Properties.Resources.HouseVip1
                            : Properties.Resources.HouseVip2,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Parent = pcbHouse,
                        Location = new Point(0, 0)
                    };
                }
                else if (level > 0)
                {
                    for (int i = 0; i < level; i++)
                    {
                        var house = new PictureBox()
                        {
                            Size = new Size(20, 20),
                            BackgroundImage = Properties.Resources.house,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Parent = pcbHouse,
                            Location = direction == 1
                                ? new Point(i * 22, (pcbHouse.Height - 20) / 2)
                                : new Point((pcbHouse.Width - 20) / 2, i * 22)
                        };
                    }
                }
            });
        }

        private async Task UpdgradeProp(int plrIndex, int propIndex)
        {
            var prop = board.properties[propIndex];
            var plr = PlayerManager.Players[plrIndex];
            double upgradeCost = (double)prop.upgrade_price! + ((double)prop.level * (double)prop.upgrade_price * 0.25);
            bool finalResult = false;
            if (prop.level >= 4) return;
            if (plrIndex == 0)
            {
                DialogResult dialogResult = MessageBox.Show($"Upgrade {prop.name} to level {prop.level + 1} for ${upgradeCost}?", "Upgrade", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    finalResult = true;
                }
            }
            else
            {
                await SocketServerManager.SocketServer.Send(SocketServerManager.ClientSockets[plr.Name], new Data(Data.ActionType.Decide, $"Upgrade {prop.name} to level {prop.level + 1} for ${upgradeCost}?"));
                _upgradeWaiter = new TaskCompletionSource<bool>();
                finalResult = await _upgradeWaiter.Task;
            }
            if (finalResult)
            {
                if (plr.PlayerMoney > upgradeCost)
                {
                    plr.PlayerMoney -= upgradeCost;
                    prop.level++;
                    await SocketServerManager.Broadcast(new Data(Data.ActionType.Upgrade, $"{plr.Name} upgraded {prop.name} to Level {prop.level} for ${upgradeCost}!", $"{plr.Name}/{plr.Position - 1}/{prop.level}/{plr.PlayerMoney}"));
                    UpdatePropertyVisual(prop.name, prop.level);
                }
                else
                {
                    if (PlayerManager.isAdmin)
                    {
                        await Task.Run(() => MessageBox.Show("You don’t have enough property value to cover this cost."));

                    }
                    else
                        await SocketServerManager.SocketServer.Send(SocketServerManager.ClientSockets[plr.Name], new Data(Data.ActionType.message, "You don’t have enough property value to cover this cost."));
                }
            }
        }
        private async Task BuyProp(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var prop = board.properties[plr.Position - 1];
            bool finalResult = false;

            if (plrIndex == 0)
            {
                DialogResult dialogResult = MessageBox.Show(
                    $"Do you want to buy {prop.name} for ${prop.price}?",
                    "Purchase Property",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (dialogResult == DialogResult.Yes)
                {
                    finalResult = true;
                }
            }
            else
            {
                await SocketServerManager.SocketServer.Send(
                    SocketServerManager.ClientSockets[plr.Name],
                    new Data(Data.ActionType.Decide, $"Do you want to buy {prop.name} for ${prop.price}?")
                );

                _buyWaiter = new TaskCompletionSource<bool>();
                finalResult = await _buyWaiter.Task;
            }

            if (finalResult)
            {
                double price = (double)prop.price!;
                if (plr.PlayerMoney >= price)
                {
                    plr.PlayerMoney -= price;
                    prop.owner = plr.Name;

                    if (panels[plr.Position - 1] != null)
                        panels[plr.Position - 1].BackColor = plr.Color;

                    await SocketServerManager.Broadcast(
                        new Data(Data.ActionType.BuyProperty,
                            $"{plr.Name} bought {prop.name} for ${price}!",
                            $"{plr.Name}/{plr.Position - 1}/{prop.level}/{plr.PlayerMoney}")
                    );
                    plr.OwnedProperties.Add(plr.Position - 1);
                }
                else
                {
                    string msg = "You don’t have enough property value to cover this cost.";

                    if (PlayerManager.isAdmin)
                    {
                        _ = Task.Run(() => MessageBox.Show(msg, "Insufficient Funds", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                    }
                    else
                    {
                        await SocketServerManager.SocketServer.Send(
                            SocketServerManager.ClientSockets[plr.Name],
                            new Data(Data.ActionType.message, msg)
                        );
                    }
                }
            }
        }
        private void RemovePropHandler(int plrIndex)
        {
            Task.Run(() =>
            {
                var plr = PlayerManager.Players[plrIndex];
                foreach (int p in plr.OwnedProperties)
                {
                    panels[p].BackColor = Color.Transparent;
                    board.properties[p].owner = null;
                    board.properties[p].level = 0;
                    UpdatePropertyVisual(board.properties[p].name, 0);
                }
            });
        }

        private async Task PayRent(int plrIndex)
        {
            var plr = PlayerManager.Players[plrIndex];
            var owner = board.properties[plr.Position - 1].owner;
            var plr1 = PlayerManager.Players.FirstOrDefault(x => x.Name == owner);
            double price = 0;
            if (board.properties[plr.Position - 1].type == "property")
            {
                price = board.properties[plr.Position - 1].levels![board.properties[plr.Position].level.ToString()];
                _ = Task.Run(() => MessageBox.Show($"{plr.Name} landed on {owner}'s property. Pay ${price} rent."));
                await SocketServerManager.Broadcast(new Data(Data.ActionType.PayRent, $"{plr.Name} landed on {owner}'s property. Pay ${price} rent./{plr.Name}/{plr1!.Name}/{plr.PlayerMoney}/{plr1.PlayerMoney}"));
            }
            else if (board.properties[plr.Position - 1].type == "railroad")
            {
                price = 1000;
                _ = Task.Run(() => MessageBox.Show($"{plr.Name} landed on {owner}'s railroad. Pay ${price}."));
                await SocketServerManager.Broadcast(new Data(Data.ActionType.PayRent, $"{plr.Name} landed on {owner}'s railroad. Pay ${price}./{plr.Name}/{plr1!.Name}/{plr.PlayerMoney}/{plr1.PlayerMoney}"));
            }
            else
            {
                await SocketServerManager.Broadcast(new Data(Data.ActionType.RollDice, PlayerManager.Players[plrIndex].Name, "Utility"));
                int[] Dices = RollDiceOnline();
                _rollWaiter = new TaskCompletionSource<bool>();
                bool check = await _rollWaiter.Task;
                if(!check) { return; }
                double multiplier = 70;
                if (plr1!.utility > 1)
                {
                    multiplier = 200;
                }
                price = (Dices[0] + Dices[1]) * multiplier;
                _ = Task.Run(() => MessageBox.Show($"{plr.Name} landed on {owner}'s Utility. Pay {multiplier} × dice roll = ${price}."));
                await SocketServerManager.Broadcast(new Data(Data.ActionType.PayRent, $"{plr.Name} landed on {owner}'s Utility. Pay {multiplier} × dice roll = ${price}./{plr.Name}/{plr1.Name}/{plr.PlayerMoney}/{plr1.PlayerMoney}"));
            }
            if (plr.PlayerMoney >= price)
            {
                plr.PlayerMoney -= price;
                plr1!.PlayerMoney += price;
            }
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
            await this.Invoke(async () =>
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
                    PlaySFX(Properties.Resources.Move, 150, 4);
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
                                    new Data(Data.ActionType.Recieve, $"{plrInfo.Name} passed GO! Collect $2000/{plrInfo.PlayerMoney}", plr)
                                );
                            }
                        }
                    }
                }
            });
        }
        private async Task PromptSellPropertiesOnl(int playerIndex, double requiredAmount = 0)
        {
            var player = PlayerManager.Players[playerIndex];
            double propertiesValue = 0;
            using (Form frm = new Form())
            {
                frm.Text = $"{player.Name} - Sell Properties";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Size = new Size(520, 420);
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = frm.MinimizeBox = false;

                ListBox lst = new ListBox()
                {
                    Location = new Point(12, 12),
                    Size = new Size(360, 320)
                };

                foreach (int idx in player.OwnedProperties.ToList())
                {
                    var prop = board.properties[idx];
                    string line = prop.level > 0
                        ? $"{prop.name} - Level {prop.level} (Sell Price: ${prop.upgrade_price})"
                        : $"{prop.name} - No houses (Price: ${prop.price})";
                    for (int i = 0; i < prop.level; i++)
                    {
                        propertiesValue += (double)prop.upgrade_price! + ((double)prop.upgrade_price * i * 0.5);
                    }
                    propertiesValue += (double)prop.sell_price!;
                    lst.Items.Add(new ListItemForSell { PropertyIndex = idx, Display = line });
                }
                if (propertiesValue < requiredAmount)
                {
                    var plr = PlayerManager.Players[playerIndex];
                    if (PlayerManager.isAdmin)
                    {
                        plr.isPlaying = false;
                        plr.IsBankrupt = true;
                        await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", plr.Name));
                    }
                    else
                    {
                        await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.Bankrupt, "", plr.Name));
                    }
                    return;
                }

                var btnSellHouse = new System.Windows.Forms.Button()
                {
                    Text = "Sell 1 House",
                    Location = new Point(380, 12),
                    Size = new Size(120, 36)
                };
                var btnSellProperty = new System.Windows.Forms.Button()
                {
                    Text = "Sell Property",
                    Location = new Point(380, 56),
                    Size = new Size(120, 36)
                };
                var btnClose = new System.Windows.Forms.Button()
                {
                    Text = "Done",
                    Location = new Point(380, 320),
                    Size = new Size(120, 36)
                };

                Label lblInfo = new Label()
                {
                    Location = new Point(12, 340),
                    Size = new Size(480, 36),
                    AutoSize = false
                };
                Action updateInfo = () =>
                {
                    lblInfo.Text = $"Money: ${player.PlayerMoney}    Required: ${(requiredAmount > 0 ? requiredAmount.ToString() : "0")}";
                };

                frm.Controls.Add(lst);
                frm.Controls.Add(btnSellHouse);
                frm.Controls.Add(btnSellProperty);
                frm.Controls.Add(btnClose);
                frm.Controls.Add(lblInfo);
                updateInfo();

                btnSellHouse.Click += async (s, e) =>
                {
                    if (lst.SelectedItem == null)
                    {
                        MessageBox.Show("Choose a property to sell house.");
                        return;
                    }
                    var item = (ListItemForSell)lst.SelectedItem;
                    var prop = board.properties[item.PropertyIndex];
                    if (prop.level <= 0)
                    {
                        MessageBox.Show("This property has no houses.");
                        return;
                    }
                    if (PlayerManager.isAdmin)
                    {
                        await SellHouseHandler(player, item.PropertyIndex);
                    }
                    else
                    {
                        await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.SellHouse, $"{item.PropertyIndex}", player.Name));
                    }

                    MessageBox.Show($"Sold 1 house at {prop.name}");
                    updateInfo();
                };

                btnSellProperty.Click += async (s, e) =>
                {
                    if (lst.SelectedItem == null)
                    {
                        MessageBox.Show("Choose a property to sell.");
                        return;
                    }
                    var item = (ListItemForSell)lst.SelectedItem;
                    var prop = board.properties[item.PropertyIndex];

                    var res = DialogResult.Yes;
                    if (prop.level > 0)
                        res = MessageBox.Show("This property has houses or hotel. Sell all?", "Confirm", MessageBoxButtons.YesNo);

                    if (res == DialogResult.Yes)
                    {
                        if (PlayerManager.isAdmin)
                        {
                            await SellPropHandler(player, item.PropertyIndex);
                        }
                        else
                        {
                            await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.SellProp, item.PropertyIndex.ToString(), player.Name));
                        }
                        MessageBox.Show($"Sold property {prop.name}");
                        updateInfo();
                        lst.Items.RemoveAt(lst.SelectedIndex);
                    }
                };

                btnClose.Click += (s, e) =>
                {
                    if (player.PlayerMoney > requiredAmount)
                    {
                        frm.DialogResult = DialogResult.OK;
                        frm.Close();
                        MessageBox.Show("You need to sell properties to continue");
                    }
                };
                frm.ShowDialog();
            }
        }
        private async void checkEndGame()
        {
            int count = 0;
            foreach (var item in PlayerManager.Players)
            {
                if (!item.IsBankrupt)
                {
                    count++;
                }
            }
            if(count == 1)
            {
                _gameLoopCts?.Cancel();
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    try { _rollWaiter?.SetResult(false); } catch { }
                    await Task.Delay(150 * 15);
                    try { _pickCardWaiter?.SetResult(false); } catch { }
                    try { _buyWaiter?.SetResult(false); } catch { }
                    try { _upgradeWaiter.SetResult(false); } catch { }
                });

                await Task.Delay(1000);

                _gameLoopCts = new CancellationTokenSource();
                _ = Task.Run(() => GameLoop(_gameLoopCts.Token));
            }
        }
        private async Task SellHouseHandler(PlayerInfo player, int index)
        {
            var prop = board.properties[index];
            player.PlayerMoney += (double)prop.upgrade_price! + ((double)prop.upgrade_price * prop.level * 0.5);
            prop.level--;
            UpdatePropertyVisual(prop.name, prop.level);
            await SocketServerManager.Broadcast(new Data(Data.ActionType.SellHouse, $"{index}/{player.PlayerMoney}/{prop.level.ToString()}", player.Name));
        }
        private async Task SellPropHandler(PlayerInfo player, int index)
        {
            var prop = board.properties[index];
            prop.owner = null;
            double totalMoney = (double)prop.sell_price!;
            panels[index].BackColor = Color.Transparent;
            player.OwnedProperties.Remove(index);
            if (prop.type == "utility")
            {
                player.utility--;
            }
            for (int i = 0; i < prop.level; i++)
            {
                totalMoney += (double)prop.upgrade_price! + ((double)prop.upgrade_price * i * 0.5);
            }
            player.PlayerMoney += totalMoney;
            await SocketServerManager.Broadcast(new Data(Data.ActionType.SellProp, $"{index}/{player.PlayerMoney}", player.Name));
            UpdatePropertyVisual(prop.name, 0);
        }

        private async void SocketServerManager_OnDataReceived(Socket client, Data data)
        {
            switch (data.Action)
            {
                case Data.ActionType.ChatMessage:
                    pnlChat.Invoke(() =>
                    {
                        txbChatHistory.AppendText($"{data.From}: {data.Msg}" + Environment.NewLine);
                    });
                    if (pnlChat.Visible == false)
                    {
                        if (!lblUnread.Visible) this.Invoke(() =>
                        {
                            lblUnread.Visible = !lblUnread.Visible;
                        });
                        unSeenMsg++;
                        this.Invoke(() => lblUnread.Text = unSeenMsg.ToString());
                    }
                    await SocketServerManager.Broadcast(client, new Data(Data.ActionType.ChatMessage, data.Msg, data.From));
                    break;
                case Data.ActionType.RollDice:
                    _rollWaiter?.SetResult(true);
                    break;
                case Data.ActionType.PickCard:
                    _pickCardWaiter?.SetResult(true);
                    break;
                case Data.ActionType.Decide:
                    {
                        _buyWaiter?.SetResult(bool.Parse(data.Msg!));
                        _upgradeWaiter?.SetResult(bool.Parse(data.Msg!));
                        break;
                    }
                case Data.ActionType.Bankrupt:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        plr!.IsBankrupt = true;
                        plr.PlayerMoney = double.MinValue;
                        await SocketServerManager.Broadcast(new Data(Data.ActionType.Bankrupt, "", data.From));
                        break;
                    }
                case Data.ActionType.SellProp:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);

                        await SellPropHandler(plr!, int.Parse(data.Msg!));
                        break;
                    }
                case Data.ActionType.SellHouse:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);

                        await SellHouseHandler(plr!, int.Parse(data.Msg!));
                        break;
                    }
            }
        }

        private async void SocketServerManager_OnClientDisconnect(Socket socket, string plrName)
        {
            await SocketServerManager.Broadcast(new Data(Data.ActionType.Disconnect, "", plrName));
            
            PlayerDisconnectHandler(plrName);
        }
        private async void PlayerDisconnectHandler(string plrName)
        {
            var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == plrName);
            if (plr == null) return;
            plr.IsBankrupt = true;
            plr.isPlaying = false;
            plr.PlayerMoney = double.MinValue;
        }

        private async void SocketClientManager_OnDataReceived(Data data)
        {
            switch (data.Action)
            {
                case Data.ActionType.SellRequest:
                    {
                        await PromptSellPropertiesOnl(int.Parse(data.From!), Math.Abs(double.Parse(data.Msg!)));
                        break;
                    }
                case Data.ActionType.message:
                    {
                        _ = Task.Run(() => MessageBox.Show(data.Msg!));
                        break;
                    }
                case Data.ActionType.SellHouse:
                    {
                        string[] values = data.Msg!.Split("/");
                        var prop = board.properties[int.Parse(values[0])];
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        plr!.PlayerMoney = double.Parse(values[1]);
                        UpdatePropertyVisual(prop.name, int.Parse(values[2]));
                        break;
                    }
                case Data.ActionType.SellProp:
                    {
                        string[] values = data.Msg!.Split("/");
                        var prop = board.properties[int.Parse(values[0])];
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        prop.owner = null;
                        panels[int.Parse(values[0])].BackColor = Color.Transparent;
                        plr!.PlayerMoney = double.Parse(values[1]);
                        plr.OwnedProperties.Remove(int.Parse(values[0]));
                        UpdatePropertyVisual(prop.name, 0);
                        break;
                    }
                case Data.ActionType.ChatMessage:
                    pnlChat.Invoke(() => txbChatHistory.AppendText($"{data.From}: {data.Msg}" + Environment.NewLine));
                    if (pnlChat.Visible == false)
                    {
                        if (!lblUnread.Visible) this.Invoke(() =>
                        {
                            lblUnread.Visible = !lblUnread.Visible;
                        });
                        unSeenMsg++;
                        this.Invoke(() => lblUnread.Text = unSeenMsg.ToString());
                    }
                    break;
                case Data.ActionType.RollDice:
                    this.Invoke(() =>
                    {
                        label1.Text = data.Msg;
                        label1.ForeColor = PlayerManager.Players.FirstOrDefault(x => x.Name == data.Msg)!.Color;

                        var plr = PlayerManager.Players.FirstOrDefault(y => y.Name == data.Msg);
                        if (plr != null)
                        {
                            if (plr.Name == PlayerConfig.playerMapping.FirstOrDefault(plr => plr.Value == PlayerManager.curPlayer).Key)
                            {
                                Dice1.Enabled = true;
                                Dice2.Enabled = true;
                            }
                            if (data.From == "Roll double to get out of jail")
                            {
                                _ = Task.Run(() => MessageBox.Show("Roll double to get out of jail."));
                            }
                            else if (data.From != null)
                            {
                                _ = Task.Run(() => MessageBox.Show("Roll the dice to determine your payment!"));
                            }
                            else
                            {
                                {
                                    _ = Task.Run(() => MessageBox.Show("Your turn"));
                                }
                            }
                        }
                    });
                    break;
                case Data.ActionType.Dice:
                    await this.Invoke(async () =>
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
                        _ = Task.Run(() => MessageBox.Show(values[0], plrInfo!.Name));
                        plrInfo!.PlayerMoney = int.Parse(values[1]);
                        break;
                    }
                case Data.ActionType.Pay:
                    {
                        var plrInfo = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        _ = Task.Run(() => MessageBox.Show(values[0], plrInfo!.Name));
                        plrInfo!.PlayerMoney = int.Parse(values[1]);
                        break;
                    }
                case Data.ActionType.GoToJail:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        int plrIndex = PlayerManager.Players.IndexOf(plr!);
                        GoToJailOnl(plrIndex);
                        if (data.Msg != null) _ = Task.Run(() => MessageBox.Show(data.Msg, plr!.Name));
                        break;
                    }
                case Data.ActionType.GetOutOfJailFree:
                    _ = Task.Run(() => MessageBox.Show(data.Msg, data.From!));
                    break;
                case Data.ActionType.PickCard:
                    this.Invoke(() =>
                    {
                        if (data.Msg == "community_chest")
                        {
                            MessageBox.Show("You landed on Fortune! Please draw a card.");
                            pcbFortune.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show("You landed on Chance! Please draw a card.");
                            pcbChance.Enabled = true;
                        }
                    });
                    break;
                case Data.ActionType.ReceiveFromEachPlayer:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        string[] values = data.Msg!.Split("/");
                        _ = Task.Run(() => MessageBox.Show(values[0], plr!.Name));
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
                        _ = Task.Run(() => MessageBox.Show(values[0], plr!.Name));
                        foreach (var p in PlayerManager.Players)
                        {
                            if (p == plr || p.IsBankrupt == true) continue;
                            p.PlayerMoney += int.Parse(values[1]);
                        }
                        plr!.PlayerMoney -= int.Parse(values[2]);
                        break;
                    }
                case Data.ActionType.PayRent:
                    {
                        string[] msg = data.Msg!.Split("/");
                        var plr1 = PlayerManager.Players.FirstOrDefault(p => p.Name == msg[1]);
                        var plr2 = PlayerManager.Players.FirstOrDefault(p => p.Name == msg[2]);
                        plr1!.PlayerMoney = double.Parse(msg[3]);
                        plr2!.PlayerMoney = double.Parse(msg[4]);
                        _ = Task.Run(() => MessageBox.Show(msg[0], plr1!.Name));
                        break;
                    }
                case Data.ActionType.Upgrade:
                    {
                        _ = Task.Run(() => MessageBox.Show(data.Msg));
                        string[] msg = data.Msg!.Split("/");
                        var plr = PlayerManager.Players.FirstOrDefault(p => p.Name == msg[0]);
                        plr!.PlayerMoney = double.Parse(msg[3]);
                        var prop = board.properties[int.Parse(msg[1])];
                        prop.level = int.Parse(msg[2]);
                        UpdatePropertyVisual(prop.name, prop.level);
                        break;
                    }
                case Data.ActionType.BuyProperty:
                    {
                        _ = Task.Run(() => MessageBox.Show(data.Msg));
                        string[] msg = data.From!.Split("/");
                        var plr = PlayerManager.Players.FirstOrDefault(p => p.Name == msg[0]);
                        plr!.PlayerMoney = double.Parse(msg[3]);
                        board.properties[int.Parse(msg[1])].owner = plr.Name;
                        panels[plr.Position - 1].BackColor = plr.Color;
                        break;
                    }
                case Data.ActionType.Decide:
                    {
                        _ = Task.Run(async () =>
                        {
                            bool fRS = false;
                            var result = MessageBox.Show(data.Msg, "", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes) fRS = true;
                            await SocketClientManager.SocketClient.Send(new Data(Data.ActionType.Decide, fRS.ToString()));
                        });
                        break;
                    }
                case Data.ActionType.Disconnect:
                    {
                        PlayerDisconnectHandler(data.From!);
                        break;
                    }
                case Data.ActionType.Bankrupt:
                    {
                        var plr = PlayerManager.Players.FirstOrDefault(x => x.Name == data.From);
                        plr!.IsBankrupt = true;
                        plr!.PlayerMoney = double.MinValue;
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
                _rollWaiter?.SetResult(true);
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
                _pickCardWaiter?.SetResult(true);
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

        private void pcbChat_Click(object sender, EventArgs e)
        {
            pnlChat.Visible = !pnlChat.Visible;
            lblUnread.Visible = false;
            unSeenMsg = 0;
        }

        private void txbChat_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                if (txbChat.Text.Trim() == "") return;
                if (PlayerManager.isOnline)
                {
                    var data = new Data(Data.ActionType.ChatMessage, txbChat.Text.Trim());
                    if (PlayerManager.isAdmin)
                    {
                        Task.Run(async () =>
                        {
                            foreach (var client in SocketServerManager.ClientSockets.Values)
                            {
                                data.From = "Player 1";
                                if (client == SocketServerManager.ClientSockets["Player 1"]) continue;
                                if (client != null)
                                {
                                    await SocketServerManager.SocketServer.Send(client, data);
                                }
                            }
                        });
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            data.From = PlayerConfig.playerMapping.FirstOrDefault(x => x.Value == PlayerManager.curPlayer).Key;
                            await SocketClientManager.SocketClient.Send(data);
                        });
                    }
                    txbChatHistory.AppendText("You: " + txbChat.Text.Trim() + Environment.NewLine);
                    txbChat.Clear();
                }
            }
        }

    }
}
