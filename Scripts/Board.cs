using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Scripts
{
    using Newtonsoft.Json.Converters;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class PropertyLevel
    {
        public int price { get; set; }
    }

    public class Property
    {
        public string name { get; set; } = null!;
        public string type { get; set; } = null!;
        public double? price { get; set; }
        public double? sell_price { get; set; }
        public double? upgrade_price { get; set; }
        public string description { get; set; } = null!;
        public Dictionary<string, double>? levels { get; set; }
        public double? amount { get; set; }
        public string? owner { get; set; }
        public int level { get; set; } = 0;
    }

    public enum CardAction
    {
        Pay,
        Receive,
        Jail,
        GetOutOfJailFree,
        PayEachPlayer,
        ReceiveFromEachPlayer
    }

    public class Card
    {
        public int id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CardAction action { get; set; }

        public int value { get; set; }
        public string description { get; set; } = string.Empty;
    }


    public class Board
    {
        public List<Property> properties { get; set; } = new(); 
        public List<Card> chance { get; set; } = new();
        public List<Card> fortune { get; set; } = new();
    }


}
