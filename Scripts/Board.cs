using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Scripts
{
    public class PropertyLevel
    {
        public int rent { get; set; }
    }

    public class Property
    {
        public string name { get; set; } = null!;
        public string type { get; set; } = null!;
        public int? price { get; set; }
        public int? sell_price { get; set; }
        public string description { get; set; } = null!;
        public Dictionary<string, PropertyLevel>? levels { get; set; }
    }

    public class Board
    {
        public List<Property> properties { get; set; } = null!;
    }

}
