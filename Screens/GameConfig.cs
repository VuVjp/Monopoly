using Monopoly.Scripts;
using Monopoly.Scripts.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Monopoly.Scripts.LookUp;

namespace Monopoly.Screens
{
    public partial class GameConfig : UserControl
    {
        private Monopoly monopoly;
        public GameConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            if (PlayerManager.isOnline)
            {
                lblTitle.Text = "Online Mode";
                btnCreate.Text = "Create Server";
                btnJoin.Text = "Join Server";
                btnDelete.Text = "Find Server";
            }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            PlayerManager.isAdmin = false;
            monopoly.LoadScreen(new PlayerConfig(monopoly));
            if (PlayerManager.isOnline)
            {
                SocketClientManager.SocketClient.Connect("", 9999);
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new MainMenu(monopoly));
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (PlayerManager.isOnline)
            {
                PlayerManager.isAdmin = true;
                monopoly.LoadScreen(new OnlineConfig(monopoly));
                return;
            }
            monopoly.LoadScreen(new PlayerConfig(monopoly));
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (PlayerManager.isOnline)
            {
                List<MyTcpInfo> myTcpInfos = BcastRequest.Lookup(1);
                lsbGameContainer.Items.Clear();
                foreach (MyTcpInfo tcpInfo in myTcpInfos)
                {
                    this.Invoke(() => lsbGameContainer.Items.Add(tcpInfo.ToString()));
                }
            }
        }
    }
}
