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
