using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.AI;
using reversi_3_player.Domain;

namespace reversi_3_player.UI
{
    public class Reversi
    {
        private GameState currentGameState = GameState.GenerateStart();

        // Główna pętla gry
        public void Run()
        {
            int i = 1;
            int playersWhoCanMove = 3;

            while (playersWhoCanMove != 0) // Rozgrywkę kończymy gdy żaden z graczy nie może się ruszyć
            {
                Console.WriteLine($"Tura {i++}:");
                currentGameState.Display();
                var nextGameState = Algorithms.MaxN(currentGameState, Heuristics.Combined);

                // Jeżeli CurrentPlayer w obecnym stanie rozgrywki nie może wykonać żadnego ruchu
                if (nextGameState == currentGameState) 
                {
                    currentGameState.SkipTurn();
                    playersWhoCanMove--;
                }
                else
                {
                    currentGameState = nextGameState;
                    playersWhoCanMove = 3;
                }

                Console.ReadLine();
            }

            Console.WriteLine("Koniec rozgrywki");
        }
    }
}
