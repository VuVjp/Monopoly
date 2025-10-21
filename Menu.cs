namespace Monopoly;

public partial class Menu : Form
{
    public Menu()
    {
        InitializeComponent();
        LoadForm();
    }

    private void LoadForm()
    {
        Forms.game = new Game();
        Forms.singleConfig = new SingleConfig();
        Forms.gameConfig = new GameConfig();
        Forms.menu = this;
    }
    private void btnQuit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnSingle_Click(object sender, EventArgs e)
    {
        Forms.isOnline = false;
        Forms.gameConfig.Show();
        this.Hide();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        MessageBox.Show(Cons.howToPlayText, "How to Play Monopoly", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
