using Monopoly.Scripts;
using Monopoly.Scripts.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly.Screens
{
    public partial class PlayerConfig : UserControl
    {
        private Monopoly monopoly;
        public static List<PictureBox> plrImgs = null!;
        public static List<PictureBox> playerBoxes = null!;
        public static Dictionary<string, PictureBox> playerMapping;
        public static Button SelectedUI = null!;

        public PlayerConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            FormLoader();
        }

        private void FormLoader()
        {
            SelectedUI = selectedUI;
            playerMapping = new Dictionary<string, PictureBox>()
            {
                { "Player1", pcbPlr1 },
                { "Player2", pcbPlr2 },
                { "Player3", pcbPlr3 },
                { "Player4", pcbPlr4 },
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

        private void OnlineModeSetup()
        {
            pcbPlr3.Visible = true;
            pcbPlr4.Visible = true;
            if (PlayerManager.isAdmin)
            {
                SocketServerManager.SocketServer.Start(IPAddress.Any.ToString(), 9999);
                SocketServerManager.isListening = true;
                SocketServerManager.isAcceptingPlayers = true;
                PlayerManager.curPlayer = pcbPlr1;
                btnPlay.Enabled = true;
                btnPlus.Enabled = true;
                btnMinus.Enabled = true;
                Task.Run(async () =>
                {
                    while (SocketServerManager.isListening)
                    {
                        string PlayerName = null!;
                        if (!SocketServerManager.isAcceptingPlayers)
                        {
                            byte[] msg = Encoding.UTF8.GetBytes("Room is full or game already started");
                            await Task.Delay(1000);
                            continue;
                        }
                        var clientSocket = await SocketServerManager.SocketServer.Accept();
                        for (int i = 1; i < SocketServerManager.ClientSockets.Count; i++)
                        {
                            if (SocketServerManager.ClientSockets["Player" + (i + 1)] == null)
                            {
                                PlayerName = "Player" + (i + 1);
                                SocketServerManager.ClientSockets["Player" + (i + 1)] = clientSocket;
                                break;
                            }
                        };
                        if (PlayerName == null)
                        {
                            byte[] msg = Encoding.UTF8.GetBytes("Room is full");
                            //await SocketServerManager.SocketServer.Send(clientSocket, msg);
                            clientSocket.Close();
                            continue;
                        }
                        SocketServerManager.ClientSockets[PlayerName] = clientSocket;
                        await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.PlayerInfo, PlayerName));
                        foreach (var pcb in PlayerConfig.playerBoxes)
                        {
                            if (pcb.Tag != null)
                            {
                                await SocketServerManager.SocketServer.Send(clientSocket, new Data(Data.ActionType.ChangeAvt, ((PictureBox)pcb.Tag!).Name, pcb.Name));
                            }
                        }
                        _ = SocketServerManager.SocketHandler(clientSocket, PlayerName);
                    }
                });
            }
            else
            {
                btnPlay.Text = "Ready";
                btnPlus.Enabled = false;
                btnMinus.Enabled = false;
                txbPlrCounter.Enabled = false;
                //Nhập IP ở đây sau
                SocketClientManager.SocketClient.Connect("", 9999);
                Task.Run(async () =>
                {
                    _ = SocketClientManager.ClientReciever();
                });
            }
        }
        private void OfflineModeSetup()
        {
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
                        if (client == SocketServerManager.ClientSockets["Player1"]) continue;
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
            if (btnPlay.Enabled == false) return;
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
            for (int i = 0; i < int.Parse(txbPlrCounter.Text); i++)
            {
                var pcb = playerBoxes[i];
                if (pcb.Tag == null)
                {
                    MessageBox.Show("All players must select an avatar!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var pictureBox = (PictureBox)pcb.Tag!;
                PlayerInfo plr = new PlayerInfo()
                {
                    Name = "Player " + (i + 1),
                    Avt = pictureBox,
                    Color = PlayerManager.colorMapping[pictureBox.Name]
                };
                PlayerManager.Players.Add(plr);
            }
            PlayerManager.playerCount = PlayerManager.Players.Count;
            monopoly.LoadScreen(new Game(monopoly));
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if( PlayerManager.isOnline && PlayerManager.isAdmin)
            {
                SocketServerManager.isListening = false;
                SocketServerManager.isAcceptingPlayers = false;
                SocketServerManager.SocketServer.Stop();
                foreach (var item in SocketServerManager.ClientSockets)
                {
                    if(item.Value != null)
                    {
                        item.Value.Dispose();
                        SocketServerManager.ClientSockets[item.Key] = null!;
                    }
                }
            }
            if( PlayerManager.isOnline && !PlayerManager.isAdmin)
            {
                SocketClientManager.SocketClient.Disconnect();
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
    }
}
