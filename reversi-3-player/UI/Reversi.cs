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
            while (true)
            {
                Console.WriteLine($"Tura {i}:");
                i++;
                currentGameState.Display();
                currentGameState = Algorithms.MaxN(currentGameState, Heuristics.PawnCount);

                Console.ReadLine();
            }
        }
    }
}
