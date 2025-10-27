using Monopoly.Screens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Monopoly.Scripts.Sockets
{
    internal class SocketClientManager
    {
        private static SocketClientManager? _socketClient;
        public static SocketClientManager SocketClient => _socketClient ??= new SocketClientManager()!;
        private Socket socketClient = null!;
        public SocketClientManager() { }

        public void Connect(string ip, int port)
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketClient.Connect(ip, port);
        }

        public async Task Send(Data data)
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

            byte[] packet = new byte[lengthPrefix.Length + payload.Length];
            Buffer.BlockCopy(lengthPrefix, 0, packet, 0, 4);
            Buffer.BlockCopy(payload, 0, packet, 4, payload.Length);

            await socketClient.SendAsync(packet);
        }

        public async Task<Data> Receive()
        {
            byte[] lengthBuffer = new byte[4];
            int length = await socketClient.ReceiveAsync(lengthBuffer);
            if (length == 0) return null!;

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] messageBuffer = new byte[messageLength];
            int totalReceived = 0;

            while (totalReceived < messageLength)
            {
                int received = await socketClient.ReceiveAsync(messageBuffer);
                if (received == 0) break;
                totalReceived += received;
            }
            return JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(messageBuffer))!;
        }

        public void Disconnect()
        {
            socketClient.Dispose();
        }
        public static async Task ClientReciever()
        {
            try
            {
                while (true)
                {
                    var received = await SocketClient.Receive();
                    if (received != null)
                    {
                        try
                        {
                            switch (received!.Action)
                            {
                                case Data.ActionType.PlayerInfo:
                                    PlayerManager.curPlayer = PlayerConfig.playerMapping[received.Msg!];
                                    PlayerConfig.SelectedUI.Location = new Point(PlayerManager.curPlayer.Location.X - 10, PlayerManager.curPlayer.Location.Y - 10);
                                    PlayerConfig.SelectedUI.Size = new Size(PlayerManager.curPlayer.Size.Width + 20, PlayerManager.curPlayer.Size.Height + 20);
                                    break;
                                case Data.ActionType.ChangeAvt:
                                    PictureBox pcbPlr = PlayerConfig.playerBoxes.FirstOrDefault(pcb => pcb.Name == received.From)!;
                                    PictureBox pcbAvt = PlayerConfig.plrImgs.FirstOrDefault(pcb => pcb.Name == received.Msg)!;
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
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SocketClient.Disconnect();
            }
        }
    }


    internal class SocketServerManager
    {
        public static bool isListening;
        public static bool isAcceptingPlayers;
        private static SocketServerManager? _socketServer;
        public static SocketServerManager SocketServer => _socketServer ??= new SocketServerManager()!;

        public static Dictionary<string, Socket> ClientSockets = new Dictionary<string, Socket>()
        {
            {"Player1", null!},
            {"Player2", null!},
            {"Player3", null!},
            {"Player4", null!}
        };
        private Socket socketServer = null!;
        public SocketServerManager() { }
        public void Start(string ip, int port)
        {
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSockets["Player1"] = socketServer;
            socketServer.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
            socketServer.Listen(4);
        }

        public async Task Send(Socket socket, Data data)
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

            byte[] packet = new byte[lengthPrefix.Length + payload.Length];
            Buffer.BlockCopy(lengthPrefix, 0, packet, 0, 4);
            Buffer.BlockCopy(payload, 0, packet, 4, payload.Length);

            await socket.SendAsync(packet);
        }
        public async Task<Data> Receive(Socket socket)
        {
            byte[] lengthBuffer = new byte[4];
            int length = await socket.ReceiveAsync(lengthBuffer);
            if (length == 0) return null!;
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] messageBuffer = new byte[messageLength];
            int totalReceived = 0;

            while (totalReceived < messageLength)
            {
                int received = await socket.ReceiveAsync(messageBuffer);
                if (received == 0) break;
                totalReceived += received;
            }

            return JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(messageBuffer))!;
        }

        public async Task<Socket> Accept()
        {
            return await socketServer.AcceptAsync();
        }
        public void Stop()
        {
            socketServer.Dispose();
        }
        public static async Task SocketHandler(Socket socket, string plr)
        {
            try
            {
                while (true)
                {
                    var received = await SocketServerManager.SocketServer.Receive(socket);
                    if (received != null)
                    {
                        switch (received.Action)
                        {
                            case Data.ActionType.ChangeAvt:
                                var pcbPlr = PlayerConfig.playerBoxes.FirstOrDefault(pcb => pcb.Name == received.From);
                                var pcbAvt = PlayerConfig.plrImgs.FirstOrDefault(pcb => pcb.Name == received.Msg);
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
                                var data2 = new Data(Data.ActionType.ChangeAvt, pcbAvt!.Name, pcbPlr!.Name);
                                foreach (var client in SocketServerManager.ClientSockets.Values)
                                {
                                    if (client == SocketServerManager.ClientSockets["Player1"]) continue;
                                    if (client != socket && client != null)
                                    {
                                        await SocketServerManager.SocketServer.Send(client, data2);
                                    }
                                }
                                break;

                        }
                    }
                    if (received == null) break;
                }
            }
            catch
            {
                SocketServerManager.ClientSockets[plr] = null!;
                socket.Dispose();
            }
        }
    }
}
