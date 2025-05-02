using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Domain;

namespace reversi_3_player.AI
{
    /// <summary>
    /// Klasa statyczna zawierająca heurystyki używane przez AI oponentów
    /// </summary>
    public static class Heuristics
    {
        public delegate double HeuristicFunc(GameState state, int player);

        public static double PawnCount(GameState state, int player)
        {
            int playerPawnCount = 0;
            int enemiesPawnCount = 0;

            for (int i = 0; i < Constants.N; i++)
            {
                for (int j = 0; j < Constants.N; j++)
                {
                    if (state.Board[i, j] == player)
                    {
                        playerPawnCount++;
                    }
                    else if (state.Board[i, j] != 0)
                    {
                        enemiesPawnCount++;
                    }
                }
            }

            return 100 * ((playerPawnCount - enemiesPawnCount) / (double)(playerPawnCount + enemiesPawnCount));
        }
    }
}
