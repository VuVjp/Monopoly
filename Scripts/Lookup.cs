using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

/*
Cách dùng cho server:
    IPEndPoint tcp_host = new IPEndPoint(IPAddress.Parse("192.168.1.254"), 3636); ip + port tùy chỉnh, nếu tcp server bind trên any thì có thể truyền any vào
    BcastListen bch = new BcastListen(tcp_host, "trandinhbac"); tên tùy chỉnh

    bch.Start() server sẽ bắt đầu nhận các broadcast của client gửi tới và server sẽ phản hồi thông tin địa chỉ + port của tcp và tên chủ server
    bch.Stop() dừng quá trình nhận broadcast lại

Cách dùng cho client:
    List<MyTcpInfo> founds = BcastRequest.Lookup(2); gửi broadcast và chờ phản hồi trong khoảng thời gian (giây). 2 giây là hợp lý
*/

namespace Monopoly.Scripts
{
    public struct MyTcpInfo
    {
        private IPEndPoint remote = new IPEndPoint(0, 0);
        private string name;

        public MyTcpInfo(byte[] packet, EndPoint from)
        {
            name = Encoding.UTF8.GetString(packet.AsSpan(12));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(packet, 8, 4);
            }

            uint addr = BitConverter.ToUInt32(packet.AsSpan(4));
            int port = BitConverter.ToInt32(packet.AsSpan(8));

            if (addr == 0)
            {
                addr = BitConverter.ToUInt32(((IPEndPoint)from).Address.GetAddressBytes());
            }

            remote = new IPEndPoint(addr, port);
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

    public class BcastListen
    {
        private Socket s;
        private Thread t;
        private string name;
        private bool is_running;
        private IPEndPoint remote_tcp;
        public static uint magic = 0xd00ddeed ^ 0xffffffff;

        private void Thread_Worker()
        {
            byte[] packet = new byte[1500];
            byte[] sign = BitConverter.GetBytes(magic);
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
                    EndPoint from = new IPEndPoint(0, 0);

                    if (s.ReceiveFrom(data, data.Length, 0, ref from) != data.Length || BitConverter.ToUInt32(data) != magic)
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
            s.Close();
        }
    }

    public class BcastRequest
    {
        static private List<MyTcpInfo> results = new List<MyTcpInfo>();
        static private readonly object locker = new object();
        static private IPAddress Calc(byte[] addr, byte[] mask)
        {
            byte[] bcast = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                bcast[i] = (byte)(mask[i] ^ 0xff | addr[i]);
            }

            return new IPAddress(bcast);
        }
        static private void Thread_Worker(byte[] addr, byte[] mask, int seconds)
        {
            byte[] resp = new byte[1500];

            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                if (s.SendTo(BitConverter.GetBytes(BcastListen.magic), sizeof(uint), 0, new IPEndPoint(BitConverter.ToUInt32(Calc(addr, mask).GetAddressBytes()), 26305)) != sizeof(uint))
                {
                    return;
                }

                while (true)
                {
                    if (!s.Poll(seconds * 1000000, SelectMode.SelectRead))
                    {
                        break;
                    }

                    EndPoint remote = new IPEndPoint(0, 0);

                    if (s.ReceiveFrom(resp, resp.Length, 0, ref remote) < 12 + 3 || BitConverter.ToInt32(resp) != BcastListen.magic)
                    {
                        continue;
                    }

                    lock (locker)
                    {
                        MyTcpInfo info = new MyTcpInfo(resp, remote);

                        string addr_info = info.GetRemote().Address.ToString();
                        int port_info = info.GetRemote().Port;

                        foreach (var res in results)
                        {
                            if (res.GetRemote().Address.ToString() == addr_info && res.GetRemote().Port == port_info)
                            {
                                s.Close();
                                return;
                            }
                        }

                        results.Add(info);
                    }
                }
    
                s.Close();
            } catch
            {
                return;
            }
        }

        static public List<MyTcpInfo> Lookup(int seconds)
        {
            if (seconds < 1)
            {
                throw new ArgumentException("Lỗi giá trị thời gian chờ!");
            }

            byte[] resp = new byte[1500];
            List<Thread> threads = new List<Thread>();

            foreach (NetworkInterface iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus != OperationalStatus.Up || iface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (var i in iface.GetIPProperties().UnicastAddresses)
                {
                    if (i.IPv4Mask.Equals(IPAddress.Any))
                    {
                        continue;
                    }

                    if (i.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    threads.Add(new Thread(() =>
                    {
                        Thread_Worker(i.Address.GetAddressBytes(), i.IPv4Mask.GetAddressBytes(), seconds);
                    }));

                    threads.Last().Start();
                }
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            return results;
        }
    }
};
