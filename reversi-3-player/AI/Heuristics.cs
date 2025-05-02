using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public static double Mobility(GameState state, int player)
        {
            int playerMovesCount = 0;
            int enemiesMovesCount = 0;

            var counted = new int[Constants.N][];
            for (int i = 0; i < Constants.N; i++)
                counted[i] = new int[Constants.N];

            for (int i = 0; i < Constants.N; i++)
            {
                for (int j = 0; j < Constants.N; j++)
                {
                    if (state.Board[i, j] == player)
                    {
                        playerMovesCount += CountPossibleMoves(state, player, (i, j), counted);
                    }
                    else if (state.Board[i, j] != 0)
                    {
                        enemiesMovesCount += CountPossibleMoves(state, state.Board[i, j], (i, j), counted);
                    }
                }
            }

            if (enemiesMovesCount + playerMovesCount != 0)
                return 100 * ((double)(playerMovesCount - enemiesMovesCount) / (double)(playerMovesCount + enemiesMovesCount));
            else
                return 0;
        }

        public static double Stability(GameState state, int player)
        {
            int playerStability = 0;
            int enemiesStability = 0;

            for (int i = 0; i < Constants.N; i++)
            {
                for (int j = 0; j < Constants.N; j++)
                {
                    if (state.Board[i, j] == player)
                    {
                        playerStability += CountStability(state, player, (i, j));
                    }
                    else if (state.Board[i, j] != 0)
                    {
                        enemiesStability += CountStability(state, state.Board[i, j], (i, j));
                    }
                }
            }

            if (playerStability + enemiesStability != 0)
                return 100 * ((double)(playerStability - enemiesStability) / (double)(playerStability + enemiesStability));
            else
                return 0;
        }

        public static double Combined(GameState state, int player)
        {
            var Pawns = PawnCount(state, player);
            var Mob = Mobility(state, player);
            var Sta = Stability(state, player);

            return Constants.weight_p * Pawns + Constants.weight_m * Mob + Constants.weight_s * Sta;
        }

        public static int CountStability(GameState state, int player, (int, int) position)
        {
            int[] DirectionStability = new int[8];
            int n = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    DirectionStability[n] = CheckStabilityDirection(state, player, position, (i, j));
                    if(n > 3)
                        if (DirectionStability[n] == -1 && DirectionStability[7 - n] == 0)
                            return -1;
                    if (i != 0 && j != 0)
                        n++;
                }
            }
            for (int i = 0; i < 4; i++)
                if (DirectionStability[n] == 0 && DirectionStability[7 - n] == 0)
                    return 0;

            return 1;
        }

        public static int CheckStabilityDirection(GameState state, int player, (int, int) position, (int,int) direction)
        {
            (int x, int y) = position;
            (int i, int j) = direction;
            x += i;
            y += j;
            while (CheckIfInSideBoard((x, y)) && state.Board[x, y] != player)
            {
                x += i;
                y += j;
            }
            if (!CheckIfInSideBoard((x, y)))
                return 1;
            if (state.Board[x, y] == 0)
                return 0;
            return -1;
        }

        public static int CountPossibleMoves(GameState state, int player, (int, int) position, int[][] counted)
        {
            int count = 0;
            for(int i = -1; i < 2;i++)
            {
                for(int j = -1;j < 2;j++)
                {
                    if (CheckIfPossibleDirection(state, player, position, (i,j), counted))
                        count++;
                }
            }
            return count;
        }

        public static bool CheckIfPossibleDirection(GameState state, int player, (int,int) position, (int,int) direction, int[][] counted)
        {
            (int x, int y) = position;
            (int i, int j) = direction;
            if (CheckIfInSideBoard((x + i, y + j)) && state.Board[x + i, y + j] != player)
                return false;
            x += 2 * i;
            y += 2 * j;
            while (CheckIfInSideBoard((x, y)) && state.Board[x, y] != player)
            {
                if (state.Board[x, y] == 0 && (counted[x][y] / Math.Pow(2 , player)) % 2 == 0)
                {
                    counted[x][y] += (int)Math.Pow(2, player);
                    return true;
                }
                x += i;
                y += j;
            }

            return false;
        }

        public static bool CheckIfInSideBoard((int x, int y) position)
        {
            return position.x >= 0 && position.x < Constants.N && position.y >= 0 && position.y < Constants.N;
        }
    }
}
