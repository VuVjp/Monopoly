using Monopoly.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Scripts
{
    class PlayerManager
    {
        public static PictureBox curPlayer = null!;
        public static PictureBox curAvt = null!;
        public static bool isOnline;
        public static bool isAdmin;
        public static int playerCount;
        public static int curPlayerIndex;
        public static Dictionary<string, Color> colorMapping = new Dictionary<string, Color>()
        {
            { "pcbAvt1", Color.Red },
            { "pcbAvt2", Color.Purple },
            { "pcbAvt3", Color.Yellow },
            { "pcbAvt4", Color.Orange },
            { "pcbAvt5", Color.Green },
            { "pcbAvt6", Color.Gray },
            { "pcbAvt7", Color.Cyan },
            { "pcbAvt8", Color.White },
        };
        public static List<PlayerInfo> Players = null!;
    }
    public class PlayerInfo
    {
        public event Action<double>? OnMoneyChanged;
        private double _money = 5000;
        public string Name = null!;
        public PictureBox Avt = null!;
        public Color Color;
        public double PlayerMoney
        {
            get => _money;
            set
            {
                if (_money != value)
                {
                    oldMoney = _money;
                    _money = value;
                    OnMoneyChanged?.Invoke(_money); 
                }
            }
        }
        public double oldMoney = 0;
        public int Position = 1;
        public bool IsBankrupt = false;
        public bool IsInJail = false;
        public int JailTurn = 0;
        public int doubleRollCount = 0;
        public int GetOutOfJailFree = 0;
        public int utility = 0;
        public bool isPlaying = true;
        public List<int> OwnedProperties = new List<int>();
    }
}
