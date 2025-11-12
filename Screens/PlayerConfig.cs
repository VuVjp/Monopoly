using Monopoly.Scripts;
using Monopoly.Scripts.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Monopoly.Screens
{
    public partial class PlayerConfig : UserControl
    {
        private Monopoly monopoly;
        public static List<PictureBox> plrImgs = null!;
        public static List<PictureBox> playerBoxes = null!;
        public static Dictionary<string, PictureBox> playerMapping;

        public PlayerConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            FormLoader();
        }

        private void FormLoader()
        {
            playerMapping = new Dictionary<string, PictureBox>()
            {
                { "Player 1", pcbPlr1 },
                { "Player 2", pcbPlr2 },
                { "Player 3", pcbPlr3 },
                { "Player 4", pcbPlr4 },
            };
            plrImgs = new List<PictureBox>() { pcbAvt1, pcbAvt2, pcbAvt3, pcbAvt4, pcbAvt5, pcbAvt6, pcbAvt7, pcbAvt8 };
            playerBoxes = new List<PictureBox> { pcbPlr1, pcbPlr2, pcbPlr3, pcbPlr4 };
            int counter = 1;
            foreach (PictureBox img in plrImgs)
            {
                img.Image = (Image)Properties.Resources.ResourceManager.GetObject("plr" +
                    counter++)!;
                img.Click += pcb_AvtClick!;
                img.MouseEnter += pcb_AvtMouseEnter!;
                img.MouseLeave += pcb_AvtMouseLeave!;
            }
            foreach (PictureBox pcb in playerBoxes)
            {
                pcb.Image = Properties.Resources.user;
            }
            if (PlayerManager.isOnline)
            {
                OnlineModeSetup();
            }
            else
            {
                OfflineModeSetup();
            }
        }
        public void TryLoadSavedGameFromFile(string savePath)
        {
            if (!File.Exists(savePath)) return;

            try
            {
                using (var con = new SQLiteConnection($"Data Source={savePath};Version=3;"))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM Players", con))
                    {
                        var reader = cmd.ExecuteReader();
                        PlayerManager.Players = new List<PlayerInfo>();
                        while (reader.Read())
                        {
                            PlayerInfo plr = new PlayerInfo(reader.GetInt32(0))
                            {
                                Name = reader["Name"].ToString(),
                                PlayerMoney = reader.GetInt32(reader.GetOrdinal("Money")),
                                IsBankrupt = reader.GetInt32(reader.GetOrdinal("IsBankrupt")) == 1,
                                Color = string.IsNullOrEmpty(reader["Color"].ToString())
                                    ? Color.Empty
                                    : ColorTranslator.FromHtml(reader["Color"].ToString()!),
                            };

                            string avatarBase64 = reader["Avatar"].ToString() ?? "";
                            if (!string.IsNullOrEmpty(avatarBase64))
                            {
                                byte[] imgBytes = Convert.FromBase64String(avatarBase64);
                                using (var ms = new MemoryStream(imgBytes))
                                {
                                    PictureBox pb = new PictureBox();
                                    pb.Image = Image.FromStream(ms);
                                    plr.Avt = pb;
                                }
                            }

                            PlayerManager.Players.Add(plr);
                        }
                    }
                }

                PlayerManager.playerCount = PlayerManager.Players.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save fail: {ex.Message}");
            }
        }

        private void OnlineModeSetup()
        {
            btnMinus.Visible = false;
            btnPlus.Visible = false;
            PlayerManager.curPlayerIndex = 1;
            txbPlrCounter.Text = $"{PlayerManager.curPlayerIndex}/{PlayerManager.playerCount}";
            if (PlayerManager.isAdmin)
            {
                SocketServerManager.OnDataReceived += HandleMessage;
                SocketServerManager.OnClientDisconnect += SocketServerManager_OnClientDisconnect;
                for (int i = 2; i < PlayerManager.playerCount; i++)
                {
                    playerBoxes[i].Visible = true;
                }
                SocketServerManager.isListening = true;
                SocketServerManager.isAcceptingPlayers = true;
                PlayerManager.curPlayer = pcbPlr1;
                btnPlay.Enabled = true;
                Task.Run(async () =>
                {
                    while (SocketServerManager.isListening)
                    {
                        string PlayerName = null!;
                        var clientSocket = await SocketServerManager.SocketServer.Accept();
                        for (int i = 1; i < SocketServerManager.ClientSockets.Count; i++)
                        {
                            if (SocketServerManager.ClientSockets["Player " + (i + 1)] == null)
                            {
                                PlayerName = "Player " + (i + 1);
                                SocketServerManager.ClientSockets["Player " + (i + 1)] = clientSocket;
                                break;
                            }
                        };
                        if (PlayerName == null || PlayerManager.playerCount == PlayerManager.curPlayerIndex)
                        {
                            await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.message, "Room is full or game already started"));
                            clientSocket.Close();
                            continue;
                        }
                        SocketServerManager.ClientSockets[PlayerName] = clientSocket;
                        await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.PlayerInfo, PlayerName));
                        await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.gameInfo, PlayerManager.playerCount.ToString(), "PlayerCount"));
                        foreach (var pcb in PlayerConfig.playerBoxes)
                        {
                            if (pcb.Tag != null)
                            {
                                await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.ChangeAvt, ((PictureBox)pcb.Tag!).Name, pcb.Name));
                            }
                        }
                        PlayerManager.curPlayerIndex++;
                        this.Invoke(new Action(() =>
                        {
                            txbPlrCounter.Text = $"{PlayerManager.curPlayerIndex}/{PlayerManager.playerCount}";
                        }));
                        foreach (var client in SocketServerManager.ClientSockets.Values)
                        {
                            if (client == SocketServerManager.ClientSockets["Player 1"]) continue;
                            if (client != null)
                            {
                                await SocketServerManager.SocketServer.Send(client, new Data(Data.ActionType.gameInfo, txbPlrCounter.Text.ToString()));
                            }
                        }
                        _ = SocketServerManager.HandleClient(clientSocket, PlayerName);
                    }
                });
            }
            else
            {
                btnPlay.Enabled = false;
                btnPlus.Visible = false;
                btnMinus.Visible = false;
                txbPlrCounter.ReadOnly = true;
                SocketClientManager.OnDataReceived += HandleNetworkMessage!;
                SocketClientManager.OnServerShutdown += SocketClientManager_OnServerShutdown;

                //Task.Run(async () =>
                //{
                //    _ = SocketClientManager.ClientReciever(monopoly, txbPlrCounter);
                //});
            }
        }

        private void SocketClientManager_OnServerShutdown(Data obj)
        {
            Task.Run(() => MessageBox.Show("Server has been shut down.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information));
            this.Invoke(new Action(() =>
            {
                SocketClientManager.OnDataReceived -= HandleNetworkMessage!;
                SocketClientManager.OnServerShutdown -= SocketClientManager_OnServerShutdown;
                monopoly.LoadScreen(new GameConfig(monopoly));
            }));
        }

        private async void SocketServerManager_OnClientDisconnect(Socket client, string plr)
        {
            PictureBox pB = PlayerConfig.playerMapping[plr];
            if (pB.Tag != null)
            {
                PictureBox oldImg = (PictureBox)pB.Tag!;
                oldImg.Tag = null;
                oldImg.BackgroundImage = null;
                pB.Tag = null;
                pB.Image = Properties.Resources.user;
            }
            foreach (var c in SocketServerManager.ClientSockets.Values)
            {
                if (c == SocketServerManager.ClientSockets["Player 1"]) continue;
                if (c != client && c != null)
                {
                    var data = new Data(Data.ActionType.ChangeAvt, "User", pB.Name);
                    await SocketServerManager.SocketServer.Send(client, data);
                }
            }
            PlayerManager.curPlayerIndex--;
            this.Invoke(new Action(() =>
            {
                txbPlrCounter.Text = $"{PlayerManager.curPlayerIndex}/{PlayerManager.playerCount}";
            }));
            foreach (var c in SocketServerManager.ClientSockets.Values)
            {
                if (c == SocketServerManager.ClientSockets["Player 1"]) continue;
                if (c != client && c != null)
                {
                    var data = new Data(Data.ActionType.gameInfo, txbPlrCounter.Text.ToString());
                    await SocketServerManager.SocketServer.Send(client, data);
                }
            }
        }

        private async void HandleMessage(Socket client, Data received)
        {
            switch (received.Action)
            {
                case Data.ActionType.ChangeAvt:
                    var pcbPlr = playerBoxes.FirstOrDefault(pcb => pcb.Name == received.From);
                    var pcbAvt = plrImgs.FirstOrDefault(pcb => pcb.Name == received.Msg);
                    if (pcbPlr != null && pcbAvt != null)
                    {
                        if (pcbPlr.Tag != null)
                        {
                            PictureBox oldImg = (PictureBox)pcbPlr.Tag!;
                            oldImg.Tag = null;
                            oldImg.BackgroundImage = null;
                        }
                        pcbPlr.Tag = pcbAvt;
                        pcbPlr.Image = pcbAvt.Image;
                        pcbAvt.Tag = pcbPlr;
                        pcbAvt.BackgroundImage = Properties.Resources.Ban;
                    }
                    await SocketServerManager.Broadcast(client, new Data(Data.ActionType.ChangeAvt, pcbAvt!.Name, pcbPlr!.Name));
                    break;
                case Data.ActionType.ChatMessage:
                    this.Invoke(new Action(() =>
                    {
                        txbChatHistory.AppendText($"{received.From}: {received.Msg}" + Environment.NewLine);
                    }));
                    await SocketServerManager.Broadcast(client, new Data(Data.ActionType.ChatMessage, received.Msg, received.From));
                    break;

            }
        }
        private void HandleNetworkMessage(Data received)
        {
            this.Invoke(new Action(() =>
            {
                switch (received.Action)
                {
                    case Data.ActionType.message:
                        MessageBox.Show(received.Msg!, "Message from server", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (received.Msg == "Room is full or game already started")
                        {
                            monopoly.LoadScreen(new GameConfig(monopoly));
                        }
                        break;
                    case Data.ActionType.gameInfo:
                        if (received.From == "PlayerCount")
                        {
                            PlayerManager.playerCount = int.Parse(received.Msg!.ToString());
                            for (int i = 2; i < PlayerManager.playerCount; i++)
                            {
                                playerBoxes[i].Visible = true;
                                playerBoxes[i].Image = Properties.Resources.user;
                            }
                            txbPlrCounter.Text = $"{PlayerManager.curPlayerIndex}/{PlayerManager.playerCount}";
                            PlayerManager.curPlayerIndex = int.Parse(received.Msg!.ToString().Split('/')[0]);
                        }
                        else
                        {
                            txbPlrCounter.Text = received.Msg!.ToString();
                            PlayerManager.curPlayerIndex = int.Parse(received.Msg!.ToString().Split('/')[0]);
                        }
                        break;
                    case Data.ActionType.PlayerInfo:
                        PlayerManager.curPlayer = playerMapping[received.Msg!];
                        selectedUI.Location = new Point(PlayerManager.curPlayer.Location.X - 10, PlayerManager.curPlayer.Location.Y - 10);
                        selectedUI.Size = new Size(PlayerManager.curPlayer.Size.Width + 20, PlayerManager.curPlayer.Size.Height + 20);
                        break;
                    case Data.ActionType.ChangeAvt:
                        if (received.Msg == "User")
                        {
                            PictureBox pcbPlrOld = PlayerConfig.playerBoxes.FirstOrDefault(pcb => pcb.Name == received.From)!;
                            if (pcbPlrOld.Tag != null)
                            {
                                PictureBox oldImg = (PictureBox)pcbPlrOld.Tag!;
                                oldImg.Tag = null;
                                oldImg.BackgroundImage = null;
                                pcbPlrOld.Tag = null;
                                pcbPlrOld.Image = Properties.Resources.user;
                            }
                            break;
                        }
                        PictureBox pcbPlr = playerBoxes.FirstOrDefault(pcb => pcb.Name == received.From)!;
                        PictureBox pcbAvt = plrImgs.FirstOrDefault(pcb => pcb.Name == received.Msg)!;
                        if (pcbPlr != null && pcbAvt != null)
                        {
                            if (pcbPlr.Tag != null)
                            {
                                PictureBox oldImg = (PictureBox)pcbPlr.Tag!;
                                oldImg.Tag = null;
                                oldImg.BackgroundImage = null;
                                pcbPlr.Tag = null;
                            }
                            pcbPlr.Tag = pcbAvt;
                            pcbPlr.Image = pcbAvt.Image;
                            pcbAvt.Tag = pcbPlr;
                            pcbAvt.BackgroundImage = Properties.Resources.Ban;
                        }
                        break;
                    case Data.ActionType.ChatMessage:
                        txbChatHistory.AppendText($"{received.From}: {received.Msg}" + Environment.NewLine);
                        break;
                    case Data.ActionType.StartGame:
                        this.Invoke(new Action(() =>
                        {
                            btnPlay_Click(null!, EventArgs.Empty);
                            SocketClientManager.OnDataReceived -= HandleNetworkMessage!;
                        }));
                        break;
                }
            }));
        }

        private void OfflineModeSetup()
        {
            pnlChat.Visible = false;
            PlayerManager.curPlayer = playerBoxes[0];
            foreach (PictureBox pcb in playerBoxes)
            {
                pcb.MouseEnter += pcb_PlayerMouseEnter!;
                pcb.MouseLeave += pcb_PlayerMouseLeave!;
                pcb.Click += pcb_PlayerClick!;
            }
        }

        private void ChangeAvatarOnline(PictureBox pcb)
        {
            var data1 = new Data(Data.ActionType.ChangeAvt, pcb.Name, PlayerManager.curPlayer.Name);
            if (PlayerManager.isAdmin)
            {
                Task.Run(async () =>
                {
                    foreach (var client in SocketServerManager.ClientSockets.Values)
                    {
                        if (client == SocketServerManager.ClientSockets["Player 1"]) continue;
                        if (client != null)
                        {
                            await SocketServerManager.SocketServer.Send(client, data1);
                        }
                    }
                });
            }
            else
            {
                Task.Run(async () =>
                {
                    await SocketClientManager.SocketClient.Send(data1);
                });
            }
        }

        private void pcb_AvtClick(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (PlayerManager.curPlayer == null) return;
            if (PlayerManager.curPlayer.Tag != null && pcb.Tag == null)
            {
                PictureBox oldImg = (PictureBox)PlayerManager.curPlayer.Tag;
                oldImg.Tag = null;
                oldImg.BackgroundImage = null;
                pcb.Tag = PlayerManager.curPlayer;
                pcb.BackgroundImage = Properties.Resources.Ban;
                PlayerManager.curPlayer.Tag = pcb;
                PlayerManager.curPlayer.Image = pcb.Image;
                if (PlayerManager.isOnline)
                {
                    ChangeAvatarOnline(pcb);
                }
                LeaveAnimation(pcb);
                return;
            }
            if (pcb.Tag == null)
            {
                PlayerManager.curPlayer.Tag = pcb.Tag;
                pcb.Tag = PlayerManager.curPlayer;
                pcb.BackgroundImage = Properties.Resources.Ban;
                PlayerManager.curPlayer.Image = pcb.Image;
                PlayerManager.curPlayer.Tag = pcb;
                if (PlayerManager.isOnline)
                {
                    ChangeAvatarOnline(pcb);
                }
                LeaveAnimation(pcb);
                return;
            }
        }
        private void pcb_AvtMouseEnter(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (pcb.Tag != null) return;
            EnterAnimation(pcb);
        }
        private void pcb_AvtMouseLeave(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            if (pcb.Tag != null) return;
            LeaveAnimation(pcb);
        }

        private void EnterAnimation(PictureBox pcb)
        {
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
            pcb.Location = new Point(pcb.Location.X - 5, pcb.Location.Y - 5);
            selectedImg.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedImg.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void LeaveAnimation(PictureBox pcb)
        {
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
            pcb.Location = new Point(pcb.Location.X + 5, pcb.Location.Y + 5);
            selectedImg.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedImg.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void pcb_PlayerClick(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            PlayerManager.curPlayer = pcb;
            selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
            selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
        }

        private void pcb_PlayerMouseEnter(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width + 10, pcb.Size.Height + 10);
            pcb.Location = new Point(pcb.Location.X - 5, pcb.Location.Y - 5);
            if (PlayerManager.curPlayer == pcb)
            {
                selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
                selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
            }
        }

        private void pcb_PlayerMouseLeave(object sender, EventArgs e)
        {
            PictureBox pcb = (PictureBox)sender;
            pcb.Size = new Size(pcb.Size.Width - 10, pcb.Size.Height - 10);
            pcb.Location = new Point(pcb.Location.X + 5, pcb.Location.Y + 5);
            if (PlayerManager.curPlayer == pcb)
            {
                selectedUI.Location = new Point(pcb.Location.X - 10, pcb.Location.Y - 10);
                selectedUI.Size = new Size(pcb.Size.Width + 20, pcb.Size.Height + 20);
            }
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            PlayerManager.Players = new List<PlayerInfo>();
            if (PlayerManager.isOnline)
            {
                if (PlayerManager.curPlayerIndex < PlayerManager.playerCount)
                {
                    MessageBox.Show("Not all players have joined!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            for (int i = 0; i < int.Parse(txbPlrCounter.Text.First().ToString()); i++)
            {
                var pcb = playerBoxes[i];
                if (pcb.Tag == null)
                {
                    MessageBox.Show("All players must select an avatar!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var pictureBox = (PictureBox)pcb.Tag!;
                PlayerInfo plr = new PlayerInfo(i + 1)
                {
                    Name = "Player " + (i + 1),
                    Avt = pictureBox,
                    Color = PlayerManager.colorMapping[pictureBox.Name]
                };
                PlayerManager.Players.Add(plr);
            }

            PlayerManager.playerCount = PlayerManager.Players.Count;
            if (PlayerManager.isOnline && PlayerManager.isAdmin)
            {
                SocketServerManager.isAcceptingPlayers = false;
                foreach (var client in SocketServerManager.ClientSockets.Values)
                {
                    if (client == SocketServerManager.ClientSockets["Player 1"]) continue;
                    if (client != null)
                    {
                        _ = SocketServerManager.SocketServer.Send(client, new Data(Data.ActionType.StartGame));
                    }
                }
                SocketServerManager.OnClientDisconnect -= SocketServerManager_OnClientDisconnect;
                SocketServerManager.OnDataReceived -= HandleMessage;
            }
            monopoly.LoadScreen(new Game(monopoly));
        }
        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (PlayerManager.isOnline && PlayerManager.isAdmin)
            {
                SocketServerManager.isListening = false;
                SocketServerManager.isAcceptingPlayers = false;
                SocketServerManager.SocketServer.Stop();
            }
            if (PlayerManager.isOnline && !PlayerManager.isAdmin)
            {
                SocketClientManager.SocketClient.Disconnect();
            }
            if(PlayerManager.isOnline)
            {
                SocketServerManager.OnDataReceived -= HandleMessage;
                SocketServerManager.OnClientDisconnect -= SocketServerManager_OnClientDisconnect;
                SocketClientManager.OnDataReceived -= HandleNetworkMessage!;
            }
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnPlus_Click(object sender, EventArgs e)
        {
            if (int.Parse(txbPlrCounter.Text) < 4)
            {
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) + 1).ToString();
                playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Visible = true;
            }
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            if (int.Parse(txbPlrCounter.Text) > 2)
            {
                if (PlayerManager.curPlayer == playerBoxes[int.Parse(txbPlrCounter.Text) - 1])
                {
                    PlayerManager.curPlayer = playerBoxes[int.Parse(txbPlrCounter.Text) - 2];
                    selectedUI.Location = new Point(PlayerManager.curPlayer.Location.X - 10, PlayerManager.curPlayer.Location.Y - 10);
                    selectedUI.Size = new Size(PlayerManager.curPlayer.Size.Width + 20, PlayerManager.curPlayer.Size.Height + 20);
                }
                if (playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag != null)
                {
                    PictureBox oldImg = (PictureBox)playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag!;
                    oldImg.Tag = null;
                    oldImg.BackgroundImage = null;
                    playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Image = Properties.Resources.user;
                    playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Tag = null;
                }
                playerBoxes[int.Parse(txbPlrCounter.Text) - 1].Visible = false;
                txbPlrCounter.Text = (int.Parse(txbPlrCounter.Text) - 1).ToString();
            }
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
                            data.From = playerMapping.FirstOrDefault(x => x.Value == PlayerManager.curPlayer).Key;
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
