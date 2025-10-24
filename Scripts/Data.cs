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
        }
        public string Msg = null!;
        public ActionType Action;
        public Data(ActionType ActionType, string msg)
        {
            Msg = msg;
            Action = ActionType;
        }
    }
}
