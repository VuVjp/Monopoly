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

namespace Monopoly.Screens
{
    public partial class GameConfig : UserControl
    {
        private Monopoly monopoly;
        public GameConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            if(PlayerManager.isOnline)
            {
                btnCreate.Text = "Create Room";
                btnJoin.Text = "Join Room";
                btnDelete.Visible = false;
            }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new PlayerConfig(monopoly));
            if (PlayerManager.isOnline)
            {
                PlayerManager.isAdmin = false;
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
    }
}
