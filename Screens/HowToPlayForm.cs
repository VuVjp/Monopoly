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
    public partial class HowToPlayForm : Form
    {
        public HowToPlayForm()
        {
            InitializeComponent();
            this.Text = "How to Play";
            this.Size = new Size(650, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ShowIcon = false; 
            this.ShowInTaskbar = false;
            Panel panel = new Panel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            Label lblTitle = new Label()
            {
                Text = "HOW TO PLAY",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10)
            };
            Label lblText = new Label()
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                MaximumSize = new Size(600, 0),
                Location = new Point(20, 80),
                Text =
                @"                               Welcome to our Monopoly-style board game!
                         You can play either **Offline** or **Online (LAN)** with friends. 
        The goal is to become the wealthiest player by buying properties, collecting rent,
                                     and managing your money wisely.


                GENERAL FEATURES
                - Play Offline or Online (LAN).
                - Choose number of players and custom avatars.
                - Offline mode supports saving and loading games.
                - Each player starts with $10,000.

                1. STARTING THE GAME
                - Choose Offline or Online (LAN) mode.
                - Select number of players and avatars.
                - Each player begins with $10,000 in cash.
                - Roll the dice to determine who goes first.

                2. MOVING AROUND THE BOARD
                - On your turn, roll two dice and move forward the total number of spaces.
                - If you roll doubles, you can roll again.
                - Rolling doubles three times in a row sends you directly to Jail.
                - Passing the GO space gives you $2,000.

                3. LANDING ON SPACES
                - GO: Collect $2,000 when you pass or land on this space.
                - Properties: Buy it if unowned. If owned, pay rent to the owner.
                - Chance & Community Chest: Draw a card and follow the instructions.
                - Income Tax: Pay 10% of your total money.
                - Luxury Tax: Pay $2,000.
                - Jail:
                  • You go to Jail if directed by the board or if you roll doubles three times.
                  • To get out of Jail, you can:
                    1. Roll doubles.
                    2. Pay $1,000.
                    3. Use a 'Get Out of Jail Free' card.

                4. BUYING AND UPGRADING PROPERTIES
                - Properties start at Level 0 (normal land).
                - You can upgrade up to Level 4:
                  • Level 1–3: Houses.
                  • Level 4: Apartment.
                - Upgrade cost formula:
                  Upgrade Price = Property Value × 1.25 × Level
                - Selling houses returns half the upgrade cost.
                - Rent increases with each level.

                5. SPECIAL PROPERTIES
                - Water & Electric Companies:
                  • Own 1: Rent = 70 × dice roll.
                  • Own both: Rent = 200 × dice roll.
                  • Cannot be upgraded.
                - Railroads (3 spaces):
                  • Have fixed rent values.
                  • Cannot be upgraded.

                6. TAX AND FEES
                - Income Tax: 10% of your money.
                - Luxury Tax: $2,000.
                - Jail Fee (optional to exit): $1,000.

                7. BANKRUPTCY
                - You are bankrupt if your money is below 0 and you have no properties left.

                8. SAVE AND LOAD (Offline Mode)
                - You can save your progress in Offline mode.
                - When reloading, all player positions, properties, and balances are restored.

                9. WINNING THE GAME
                - The game ends when all other players are bankrupt.

                "
                            };

                panel.Controls.Add(lblText);
            panel.Controls.Add(lblTitle);
            this.Controls.Add(panel);
        }
    }
}