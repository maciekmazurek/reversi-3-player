using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Domain;

namespace reversi_3_player
{
    public class Reversi
    {
        private GameState currentGameState = GameState.GenerateStart();

        public void Run()
        {
            Console.WriteLine("Choose how you want to play:\n" +
                "1 - if you want to play with AI\n" +
                "2 - if you want to see AI playing");
            while (true)
            {
                var res = Console.ReadLine();

                if (res == "1")
                {
                    PlayertoAI();
                    break;
                }
                else if(res == "2")
                {
                    AItoAI();
                    break;
                }
            }
        }

        public void PlayertoAI()
        {
            Console.WriteLine("You are playing blacks, to choose field type RC where R is row number and C is column number");
            int i = 1;
            while (true)
            {
                Console.WriteLine($"Tura {i}:");
                
                currentGameState.Display();
                
                if (currentGameState.CurrentPlayer == 1)
                {
                    GameState? newState = null;
                    while (newState == null)
                    {
                        var res = Console.ReadLine();
                        if (res.Length != 2)
                        {
                            Console.WriteLine("Invalid arguments length\n");
                            continue;
                        }
                        int x = res[0] - '0';
                        int y = res[1] - '0';
                        newState = currentGameState.PlayerTryToPlacePawn((x, y));
                        if (newState == null)
                            Console.WriteLine("Invalid arguments\n");
                    }

                    currentGameState = newState;
                }
                else
                {
                    currentGameState = Algorithms.MaxN(currentGameState, Heuristics.Combined);
                    Console.ReadLine();
                }

                i++;
            }
        }

        public void AItoAI()
        {
            int i = 1;
            while (true)
            {
                Console.WriteLine($"Tura {i}:");
                i++;
                currentGameState.Display();
                currentGameState = Algorithms.MaxN(currentGameState, Heuristics.Combined);

                Console.ReadLine();
            }
        }
    }
}
