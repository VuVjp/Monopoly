using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
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
            result =  x.Next(6);
            return result;
            
        }
        static void EndTurn()
        {

        }
        static void LoadPlayer()
        {
            for (int i = 0; i < PlayerManager.PlayerCount; i++)
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
