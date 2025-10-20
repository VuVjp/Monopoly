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
        Forms.menu = this;
    }
    private void ChangedForm(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        switch (btn.Name)
        {
            case "btnSingle":
                btnSingle.BackColor = Color.LightGray;
                btnOnline.BackColor = Color.White;
                break;
            case "btnOnline":
                btnOnline.BackColor = Color.LightGray;
                btnSingle.BackColor = Color.White;
                break;
        }
    }   
    private void btnQuit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnSingle_Click(object sender, EventArgs e)
    {
        Forms.isOnline = false;
        Forms.game.Show();
        this.Hide();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        MessageBox.Show(Cons.howToPlayText, "How to Play Monopoly", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
