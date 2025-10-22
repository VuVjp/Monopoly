using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    internal class Forms
    {
        internal static bool isOnline;
        internal static string gameInfoText = null!;
        internal static Game game = null!;
        internal static Menu menu = null!;
        internal static GameConfig gameConfig = null!;
        internal static PlayerConfig singleConfig = null!;
        internal static bool isAdmin;

        public static void Dispose()
        {
            menu.Dispose();
            gameConfig.Dispose();
            singleConfig.Dispose();
            game.Dispose();
        }
    }
}
