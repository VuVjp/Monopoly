using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    internal class Cons
    {
        internal const string howToPlayText = "Welcome to Monopoly!\n\n" +
            "Objective:\n" +
            "Be the last player remaining with money by buying, trading, and developing properties.\n\n" +
            "How to Play:\n" +
            "1. Roll the dice to move around the board.\n" +
            "2. Buy properties when you land on them.\n" +
            "3. Collect rent from opponents who land on your properties.\n" +
            "4. Build houses and hotels to increase rent.\n" +
            "5. Use Chance and Community Chest cards for special actions.\n\n" +
            "Special Spaces:\n" +
            "- Go: Collect $200 each time you pass.\n" +
            "- Jail: You can get out by rolling doubles, using a 'Get Out of Jail Free' card, or paying $50.\n" +
            "- Free Parking: A safe space with no penalties or rewards.\n\n" +
            "Winning the Game:\n" +
            "The game ends when all other players are bankrupt. The last player remaining wins the game!\n\n" +
            "Have fun and good luck!";

        internal const uint signature = 0xd00ddeed ^ 0xffffffff;
    }
}
