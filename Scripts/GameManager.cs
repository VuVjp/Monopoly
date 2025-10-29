using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Scripts
{
    interface GameManager
    {
        static void StartGame()
        {

        }
        static void StopGame() 
        { 

        }
        static int RollDice()
        {

            int result = 0;
            Random x = new Random();
            result = x.Next(1,7);
            return result;

        }
        static void EndTurn()
        {

        }
        static void LoadPlayerInMap()
        {

        }
        static void LoadPlayer()
        {
            for (int i = 0; i < PlayerManager.playerCount; i++)
            {
                switch (i)
                {
                    case 0:
                        break;
                }
            }
        }

    }
    public class OfflineGameManager : GameManager
    {
    }
    public class OnlineGameManager : GameManager
    {
    }
}
