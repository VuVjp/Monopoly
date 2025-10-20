namespace Monopoly;

public partial class Menu : Form
{
    public Menu()
    {
        InitializeComponent();
        Load();
    }

    private void Load()
    {
        Forms.game = new Game();
        Forms.menu = this;
    }   
    private void btnQuit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnSingle_Click(object sender, EventArgs e)
    {
        Forms.game.Show();
        this.Hide();
    }
}
