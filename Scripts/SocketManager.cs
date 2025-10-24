using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public async Task Send(byte[] data)
        {
            await socketClient.SendAsync(data);
        }

        public async Task<int> Receive(byte[] buffer)
        {
            return await socketClient.ReceiveAsync(buffer);
        }

        public void Disconnect()
        {
            socketClient.Dispose();
        }
    }
    internal class SocketServerManager
    {
        private static SocketServerManager? _socketServer;
        public static SocketServerManager SocketServer => _socketServer ??= new SocketServerManager()!;
        private Socket socketServer = null!;
        public SocketServerManager() { }
        public void Start(string ip, int port)
        {
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
            socketServer.Listen(4);
        }
        public Socket Accept()
        {
            return socketServer.Accept();
        }
        public void Stop()
        {
            socketServer.Dispose();
        }
    }
}
