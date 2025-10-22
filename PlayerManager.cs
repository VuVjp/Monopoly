using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    class PlayerManager
    {
        public static int PlayerCount;
    }
    class PlayerInfo
    {
        public string Name = null!; 
        public PictureBox Avt = null!;
        public Color Color;
    }
    class PlayerInfoManager
    {
        internal static Dictionary<string, PlayerInfo> plrInfo = null!;
    }
    class PlayerColor
    {
        internal readonly Color Red = Color.Red;
        internal readonly Color Green = Color.Green;
        internal readonly Color Blue = Color.Cyan;
        internal readonly Color Yellow = Color.Yellow;
        internal readonly Color Pink = Color.Pink;
        internal readonly Color Orange = Color.Orange;
        internal readonly Color Black = Color.Black;
        internal readonly Color White = Color.White;
    }
}
