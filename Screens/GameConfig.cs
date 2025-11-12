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

        private List<MyTcpInfo> myTcpInfos;
        private bool isRefreshing = false;
        public GameConfig(Monopoly monopoly)
        {
            InitializeComponent();
            this.monopoly = monopoly;
            if (!PlayerManager.isOnline)
            {
                LoadSaveList();
                btnCreate.Text = "New Game";
                btnJoin.Text = "Load Game";
            }
            if (PlayerManager.isOnline)
            {
                lblTitle.Text = "Online Mode";
                btnCreate.Text = "Create Server";
                btnJoin.Text = "Join Directly";
                btnDelete.Text = "Refesh";
                lsbGameContainer.DoubleClick += LsbGameContainer_DoubleClick;
                _ = Task.Run(async() =>
                {
                    myTcpInfos = BcastRequest.Lookup(1);

                    lsbGameContainer.Items.Clear();
                    foreach (MyTcpInfo tcpInfo in myTcpInfos)
                    {
                        this.Invoke(() => lsbGameContainer.Items.Add(tcpInfo.ToString()));
                    }
                    await Task.Delay(1000);
                });
            }
        }

        private void LsbGameContainer_DoubleClick(object? sender, EventArgs e)
        {
            ListBox listBox = (ListBox)sender!;
            if (listBox.SelectedItem == null) return;
            var endPoint = myTcpInfos[listBox.SelectedIndex].GetRemote();
            PlayerManager.isAdmin = false;
            monopoly.LoadScreen(new PlayerConfig(monopoly));
            SocketClientManager.SocketClient.Connect(endPoint.Address.ToString(), endPoint.Port);
        }

        private void LoadSaveList()
        {
            lsbGameContainer.Items.Clear();
            string saveDir = Application.StartupPath;
            string[] saveFiles = Directory.GetFiles(saveDir, "save_*.sqlite");
            Array.Sort(saveFiles);
            foreach (var file in saveFiles)
                lsbGameContainer.Items.Add(Path.GetFileName(file));

            if (lsbGameContainer.Items.Count > 0)
                lsbGameContainer.SelectedIndex = 0;
        }

        private string GenerateSaveFileName()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(Application.StartupPath, $"save_{timestamp}.sqlite");
        }
        private void btnJoin_Click(object sender, EventArgs e)
        {

            if (!PlayerManager.isOnline && lsbGameContainer.Items.Count > 0)
            {
                if (lsbGameContainer.SelectedItem == null)
                {
                    MessageBox.Show("Please select a save file first!");
                    return;
                }
                string selectedFile = Path.Combine(Application.StartupPath, lsbGameContainer.SelectedItem.ToString()!);
                PlayerManager.IsLoadingSavedGame = true;
                PlayerConfig playerConfig = new PlayerConfig(monopoly);
                playerConfig.TryLoadSavedGameFromFile(selectedFile);
                monopoly.LoadScreen(new Game(monopoly, selectedFile));
                return;
            }
            if (PlayerManager.isOnline)
            {
                PlayerManager.isAdmin = false;

                monopoly.LoadScreen(new FindServer(monopoly));
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
            if (lsbGameContainer.Items.Count > 0)
            {
                string selectedFile = Path.Combine(Application.StartupPath, lsbGameContainer.SelectedItem?.ToString() ?? "");

                if (File.Exists(selectedFile))
                {
                    var dr = MessageBox.Show(
                        "A save file is detected. Do you want to continue the saved game?",
                        "Load Game",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (dr == DialogResult.Yes)
                    {
                        PlayerManager.IsLoadingSavedGame = true;
                        monopoly.LoadScreen(new Game(monopoly, selectedFile));
                        return;
                    }
                }
            }
            PlayerManager.IsLoadingSavedGame = false;
            monopoly.LoadScreen(new PlayerConfig(monopoly));
        }
        public string GetNewSavePath() => GenerateSaveFileName();

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (PlayerManager.isOnline)
            {
                if (!isRefreshing)
                {
                    _ = Task.Run(() =>
                    {
                        isRefreshing = true;
                        myTcpInfos = BcastRequest.Lookup(1);

                        lsbGameContainer.Items.Clear();
                        foreach (MyTcpInfo tcpInfo in myTcpInfos)
                        {
                            this.Invoke(() => lsbGameContainer.Items.Add(tcpInfo.ToString()));
                        }
                        isRefreshing = false;
                    });
                }
            }
            else
            {
                if (lsbGameContainer.SelectedItem == null)
                {
                    MessageBox.Show("Please select a save file to delete!");
                    return;
                }

                string selectedFile = Path.Combine(Application.StartupPath, lsbGameContainer.SelectedItem.ToString()!);

                var dr = MessageBox.Show(
                    $"Are you sure you want to delete '{Path.GetFileName(selectedFile)}'?",
                    "Delete Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (dr == DialogResult.Yes)
                {
                    try
                    {
                        if (File.Exists(selectedFile))
                            File.Delete(selectedFile);

                        LoadSaveList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
