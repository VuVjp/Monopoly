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
            Receive,
            PickCard,
            Upgrade,
            Auction,
            Bankrupt,
            PlayerInfo,
            ChatMessage,
            PlayerJoin,
            ChangeAvt,
            Dice,
            GetAvtList, 
            PayEachPlayer,
            GoToJail,
            ReceiveFromEachPlayer,
            GetOutOfJailFree,
            PayRent,
            Decide,
            Disconnect,
            SellProp,
            SellHouse,
            SellRequest,
            GetOutOfJails,
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
