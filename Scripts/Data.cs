using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Scripts
{
    internal class Data
    {
        public enum ActionType
        {
            gameInfo,
            message,
            StartGame,
            EndGame,
            RollDice,
            LoadPlayer,
            BuyProperty,
            Pay,
            Recieve,
            PickCard,
            Upgrade,
            Auction,
            Bankrupt,
            PlayerInfo,
            ChatMessage,
            PlayerJoin,
            ChangeAvt,
            GetAvtList
        }
        public string? Msg = null!;
        public string? From = null!; 
        public ActionType Action;
       
        public Data(ActionType ActionType, string? msg = null!,string? from = null!)
        {
            Msg = msg;
            From = from;
            Action = ActionType;
        }
    }
}
