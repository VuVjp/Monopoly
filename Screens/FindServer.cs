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
    public partial class FindServer : UserControl
    {
        private Monopoly monopoly;
        public FindServer(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
        }

        private void btReturn_Click(object sender, EventArgs e)
        {
            monopoly.LoadScreen(new GameConfig(monopoly));
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {

            if (tbxIP.Text == "")
            {
                MessageBox.Show("Please enter the server IP address.", "Missing IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txbPort.Text == "")
            {
                MessageBox.Show("Please enter the port number before connecting.", "Missing Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            try
            {
                monopoly.LoadScreen(new PlayerConfig(monopoly));
                SocketClientManager.SocketClient.Connect(tbxIP.Text, int.Parse(txbPort.Text));
            }
            catch
            {
                _ = Task.Run(()=>MessageBox.Show("Failed to connect to the server. Please check the IP address and port, then try again.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                monopoly.LoadScreen(new FindServer(monopoly));
            }

        }
    }
}
