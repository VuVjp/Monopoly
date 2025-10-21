using System.Net;
using System.Text;
using System.Net.Sockets;
using Monopoly;

/*
Cách dùng cho server:
    IPEndPoint tcp_host = new IPEndPoint(IPAddress.Parse("192.168.1.254"), 3636); ip + port tùy chỉnh
    BcastListen bch = new BcastListen(tcp_host, "trandinhbac"); tên tùy chỉnh

    bch.Start() server sẽ bắt đầu nhận các broadcast của client gửi tới và server sẽ phản hồi thông tin địa chỉ + port của tcp và tên chủ server
    bch.Stop() dừng quá trình nhận broadcast lại

Cách dùng cho client:
    BcastRequest bch = new BcastRequest();
    List<MyTcpInfo> founds = bch.Lookup(10); gửi broadcast và chờ phản hồi trong khoảng thời gian (giây). 2 giây là hợp lý
*/

namespace LookupHelper
{
    public class BcastListen
    {
        private Socket s;
        private Thread t;
        private string name;
        private bool is_running;
        private IPEndPoint remote_tcp;

        private void Thread_Worker()
        {
            byte[] packet = new byte[1500];
            byte[] sign = BitConverter.GetBytes(Cons.signature);
            byte[] addr = remote_tcp.Address.GetAddressBytes();
            byte[] port = BitConverter.GetBytes(remote_tcp.Port);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(port);
            }

            Buffer.BlockCopy(sign, 0, packet, 0, sign.Length);
            Buffer.BlockCopy(addr, 0, packet, sign.Length, addr.Length);
            Buffer.BlockCopy(port, 0, packet, sign.Length + addr.Length, port.Length);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(name), 0, packet, sign.Length + addr.Length + port.Length, name.Length);

            while (is_running)
            {
                try
                {
                    byte[] data = new byte[sizeof(uint)];

                    IPEndPoint from_buffer = new IPEndPoint(0, 0);
                    EndPoint from = (EndPoint)from_buffer;

                    if (s.ReceiveFrom(data, data.Length, 0, ref from) != data.Length || BitConverter.ToUInt32(data) != Cons.signature)
                    {
                        continue;
                    }

                    if (s.SendTo(packet, 12 + name.Length, 0, from) != 12 + name.Length)
                    {
                        continue;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public void Start()
        {
            is_running = true;
            t.Start();
        }

        public void Stop()
        {
            is_running = false;
        }

        public BcastListen(IPEndPoint tcp_addr, string master_name)
        {
            if (master_name.Length < 3 || master_name.Length > 15)
            {
                throw new ArgumentException("Lỗi tên!");
            }

            remote_tcp = tcp_addr;
            name = master_name;

            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.Bind(new IPEndPoint(IPAddress.Any, 26305));

            t = new Thread(new ThreadStart(Thread_Worker));
        }

        ~BcastListen()
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }

    public struct MyTcpInfo
    {
        private IPEndPoint remote = new IPEndPoint(0, 0);
        private string name;

        public MyTcpInfo(byte[] packet)
        {
            name = Encoding.UTF8.GetString(packet.AsSpan(12));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(packet, 8, 4);
            }

            remote = new IPEndPoint(
                BitConverter.ToUInt32(packet.AsSpan(4)),
                BitConverter.ToInt32(packet.AsSpan(8))
            );
        }

        public IPEndPoint GetRemote()
        {
            return remote;
        }
        public string GetName()
        {
            return name;
        }
        
    };

    public class BcastRequest
    {
        private Socket s;
        public BcastRequest()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        }

        public List<MyTcpInfo> Lookup(int seconds)
        {
            if (seconds < 1)
            {
                throw new ArgumentException("Lỗi giá trị thời gian chờ!");
            }

            byte[] resp = new byte[1500];

            IPEndPoint bcast_addr = new IPEndPoint(IPAddress.Broadcast, 26305);
            s.SendTo(BitConverter.GetBytes(Cons.signature), sizeof(uint), 0, bcast_addr);

            List<MyTcpInfo> founds = new List<MyTcpInfo>();

            while (true)
            {
                if (!s.Poll(seconds * 1000000, SelectMode.SelectRead))
                {
                    break;
                }

                if (s.Receive(resp, resp.Length, 0) < 12 + 3 || BitConverter.ToInt32(resp) != Cons.signature)
                {
                    continue;
                }

                founds.Add(new MyTcpInfo(resp));
            }

            return founds;
        }

        ~BcastRequest()
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }
};
