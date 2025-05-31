using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using reversi_3_player.Domain;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace reversi_3_player.AI
{
    /// <summary>
    /// Klasa zawierająca heurystyki używane przez AI oponentów
    /// </summary>
    public class Heuristics
    {
        double wPawn = 0.33;
        double wMob = 0.33;
        double wStab = 0.33;

        public Heuristics(double w1, double w2, double w3)
        {
            wPawn = w1;
            wMob = w2;
            wStab = w3;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static double Stability(GameState state, int player)
        {
            int n = Constants.N;
            int playerStability = 0;
            int enemiesStability = 0;

            int positivStab = 0;

            var pawnsStability = new int[n][];

            for (int i = 0; i < n; i++)
            {
                pawnsStability[i] = new int[n];
                for (int j = 0; j < n; j++)
                    pawnsStability[i][j] = -2;
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (pawnsStability[i][j] == -2)
                    {
                        if (state.Board[i, j] == player)
                        {
                            var res = CountStability2(state, player, (i, j), pawnsStability);
                            if (res == 1)
                            {
                                //Console.WriteLine($"({i}, {j})");
                                positivStab++;
                            }
                            playerStability += res;
                            pawnsStability[i][j] = res;
                        }
                        else if (state.Board[i, j] != 0)
                        {
                            var res = CountStability2(state, state.Board[i, j], (i, j), pawnsStability);
                            enemiesStability += res;
                            pawnsStability[i][j] = res;
                        }
                    }
                    else
                    {
                        if (state.Board[i, j] == player)
                        {
                            playerStability += pawnsStability[i][j];
                        }
                        else if (state.Board[i, j] != 0)
                        {
                            enemiesStability += pawnsStability[i][j];
                        }
                    }
                }
            }

            //if (positivStab > 4)
            //{
            //    Console.WriteLine($"{positivStab}");
            //    Console.WriteLine("\n" + player + " " + playerStability + "\n");
            //    state.Display();
            //}

            //Console.WriteLine("\n" + player + " " + playerStability + "\n");



            if (playerStability + enemiesStability != 0)
                return 100 * ((double)(playerStability - enemiesStability) / (double)(playerStability + enemiesStability));
            else
                return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public double Combined(GameState state, int player)
        {
            var Pawns = PawnCount(state, player);
            var Mob = Mobility(state, player);
            var Sta = Stability(state, player);

            return wPawn * Pawns + wMob * Mob + wStab * Sta;
            //return Constants.weight_p * Pawns + Constants.weight_m * Mob + Constants.weight_s * Sta;
        }

        /// <summary>
        /// Oblicza stabilność danego gracza na danej pozycji
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <returns></returns>

        public static int CountStability2(GameState state, int player, (int, int) position, int[][] pawnsStability)
        {
            int enemy1 = (player + 1) % 3;
            enemy1 = enemy1 == 0 ? 3 : enemy1;
            int enemy2 = (player + 2) % 3;
            enemy2 = enemy2 == 0 ? 3 : enemy2;

            var Stability1 = CountStabilityOneEnemy(state, player, enemy1, position, pawnsStability);

            var Stability2 = CountStabilityOneEnemy(state, player, enemy2, position, pawnsStability);

            return Stability1 < Stability2 ? Stability1 : Stability2;
        }

        public static int CountStabilityOneEnemy(GameState state, int player, int enemy, (int, int) position, int[][] pawnsStability)
        {
            var DirectionStability = new int[4];

            bool Zero = false;

            for (int k = 0; k < 4; k++)
            {
                (int i, int j) = GetDirection(k);
                (int x, int y) = position;

                //var Out = new bool[2];
                //var Empty = new bool[2];
                //var Enemy = new bool[2];
                (var Out1, var Out2) = (false, false);
                (var Empty1, var Empty2) = (false, false);
                (var Enemy1, var Enemy2) = (false, false);

                (Out1, Empty1, Enemy1) = CheckStabilityDirection2(state, player, enemy, position, (i, j));

                (Out2, Empty2, Enemy2) = CheckStabilityDirection2(state, player, enemy, position, (-i, -j));


                if ((Enemy1 && !Enemy2 && Empty2) || (Enemy2 && !Enemy1 && Empty1))
                    DirectionStability[k] = -1;
                else if (Empty2 || Empty1)
                {
                    if (Empty2 && Empty1)
                        DirectionStability[k] = 0;
                    else
                    {
                        //Console.WriteLine("sadasd");
                        pawnsStability[x][y] = 1;
                        (var positiveDirectionStab, var stableEnemyPositive) = CheckPawnsStabilityInDirection(state, player, enemy, position, (i, j), pawnsStability);
                        (var negativeDirectionStab, var stableEnemyNegative) = CheckPawnsStabilityInDirection(state, player, enemy, position, (-i, -j), pawnsStability);
                        pawnsStability[x][y] = -2;

                        if ((positiveDirectionStab && negativeDirectionStab && Enemy1 && Enemy2) ||
                            (positiveDirectionStab && !Enemy1) || (negativeDirectionStab && !Enemy2) ||
                            (stableEnemyNegative && stableEnemyPositive))
                            DirectionStability[k] = 1;
                        else
                            DirectionStability[k] = 0;
                    }
                }
                else
                    DirectionStability[k] = 1;

                if (DirectionStability[k] == -1)
                    return -1;
                if (DirectionStability[k] == 0)
                    Zero = true;
            }

            if (Zero)
                return 0;

            return 1;
        }

        public static (bool, bool) CheckPawnsStabilityInDirection(GameState state, int player, int enemy, (int, int) position, (int, int) direction, int[][] pawnsStability)
        {
            (int x, int y) = position;
            (int i, int j) = direction;
            x += i;y += j;

            bool stableEnemy = false;
            bool stable = true;

            while(CheckIfInSideBoard((x, y)) && state.Board[x,y] > 0)
            {
                int stability = pawnsStability[x][y];
                if (stability == -2)
                {
                    stability = CountStability2(state, state.Board[x, y], (x, y), pawnsStability);
                    pawnsStability[x][y] = stability;
                }
                if (stability != 1)
                    stable = false;
                else if (state.Board[x, y] == enemy)
                    stableEnemy = true;

                x += i;
                y += j;
            }

            return (stable, stableEnemy);
        }

        //public static bool 

        public static (int x, int y) GetDirection(int x)
        {
            switch(x)
            {
                case 0:
                    return (0, 1); // w prawo
                case 1:
                    return (1, 1); // w prawo i w górę
                case 2:
                    return (1, 0); // w górę
                case 3:
                    return (1, -1); // w lewo i w górę
                default:
                    return (0, 0);
            }
        }

        public static int CountStability(GameState state, int player, (int, int) position)
        {
            int[] DirectionStability = new int[4];

            int player2 = (player + 1) % 3;
            int player3 = (player + 2) % 3;
            player2 = player2 == 0 ? 3 : player2;
            player3 = player3 == 0 ? 3 : player3;

            bool Zero = false;

            for (int k = 0; k < 4; k++)
            {
                (int i, int j) = GetDirection(k);
                (int x, int y) = position;

                var Out = new bool[2];
                var Empty = new bool[2];
                var P2 = new int[2];
                var P3 = new int[2];

                (Out[0], Empty[0], P2[0], P3[0]) = CheckStabilityDirection(state, player, player2, player3, position, (i, j));

                (Out[1], Empty[1], P2[1], P3[1]) = CheckStabilityDirection(state, player, player2, player3, position, (-i, -j));

                if ((Out[0] && Out[1]) || (Out[0] && P2[0] == 0 && P3[0] == 0) || (Out[1] && P2[1] == 0 && P3[1] == 0))
                    DirectionStability[k] = 1;
                else if (((P2[0] == 0 && P2[1] == 0) || (P2[0] > 0 && P2[1] > 0)) &&
                        ((P3[0] == 0 && P3[1] == 0) || (P3[0] > 0 && P3[1] > 0)))
                {
                    if (Out[0] || Out[1])
                        DirectionStability[k] = 1;
                    else
                        DirectionStability[k] = 0;
                }
                else
                {
                    DirectionStability[k] = -1;
                    //for (int l = 0; l < 2; l++)
                    //{
                    //    int nextl = (l + 1) % 2;
                    //    if ((P2[l] > 0 && P2[nextl] == 0 && Empty[nextl]) || (P3[l] > 0 && P3[nextl] == 0 && Empty[nextl]))
                    //        DirectionStability[k] = -1;
                    //}
                }

                if (DirectionStability[k] == -1)
                    return -1;
                if (DirectionStability[k] == 0)
                    Zero = true;
            }

            if (Zero)
                return 0;

            return 1;
        }

        public static double Stability2(GameState state, int player)
        {
            int playerStability = 0;
            int enemiesStability = 0;

            int n = Constants.N;

            var PawnsStability = new int[n][];
            for (int i = 0; i < n; i++)
                PawnsStability[i] = new int[n];

            for (int i = 0; i < 4; i++)
            {
                (int x, int y) = GetCorner(i);

            }

            if (playerStability + enemiesStability != 0)
                return 100 * ((double)(playerStability - enemiesStability) / (double)(playerStability + enemiesStability));
            else
                return 0;
        }

        public static void CountCornersStability(GameState state, int[][] PawnsStability)
        {
            (int x, int y) = (0, 0);

            int n = Constants.N;

            int depth = 0;

            while (depth < 4)
            {
                int player = state.Board[x, y];
                if (player == 0)
                    continue;

                bool allDirectionsStable = true;

                for (int k = 0; k < 4; k++)
                {
                    (int i, int j) = GetDirection(k);
                    if (CheckIfInSideBoard((x + i, y + j)) && CheckIfInSideBoard((x - i, y - j)) &&
                        (PawnsStability[x + i][y + j] != 1 || state.Board[x + i, y + j] != player) &&
                        (PawnsStability[x - i][y - j] != 1 || state.Board[x - i, y - j] != player))
                        allDirectionsStable = false;
                }

                if (allDirectionsStable)
                    PawnsStability[x][y] = 1;

                if (x == 0 + depth)
                {
                    if (y + 1 == n - depth)
                        x++;
                    else
                        y++;
                }
                else if (x + 1 == n - depth)
                {
                    if (y == 0 + depth)
                        x--;
                    else
                        y--;
                }
                else
                {
                    if (y + 1 == n - depth)
                        x++;
                    else if (y == 0 + depth)
                        x--;
                }

                if ((x, y) == (depth, depth))
                {
                    depth++;
                    (x, y) = (depth, depth);
                }
            }
        }

        public static (int x, int y) GetCorner(int x)
        {
            int n = Constants.N - 1;
            switch (x)
            {
                case 0:
                    return (0, n); // w prawo
                case 1:
                    return (n, n); // w prawo i w górę
                case 2:
                    return (n, 0); // w górę
                case 3:
                    return (0, 0); // w lewo i w górę
                default:
                    return (-1, -1);
            }
        }

        /// <summary>
        /// Oblicza stabilność w danym kierunku
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns></returns>

        public static (bool Out, bool Empty, int P2, int P3) CheckStabilityDirection(GameState state, int player, int player2, int player3, (int, int) position, (int, int) direction)
        {
            (int x, int y) = position;
            (int i, int j) = direction;

            int p2 = 0;
            int p3 = 0;

            while (CheckIfInSideBoard((x, y)) && state.Board[x, y] != 0)
            {
                if (state.Board[x, y] == player2)
                    p2++;
                else if (state.Board[x, y] == player3)
                    p3++;
                x += i;
                y += j;
            }

            if (!CheckIfInSideBoard((x, y)))
                return (true, false, p2, p3);

            return (false, true, p2, p3);
        }

        public static (bool Out, bool Empty, bool EnemyPawn) CheckStabilityDirection2(GameState state, int player, int enemy, (int, int) position, (int, int) direction)
        {
            (int x, int y) = position;
            (int i, int j) = direction;
            bool EnemyPawn = false;

            while (CheckIfInSideBoard((x, y)) && state.Board[x, y] != 0)
            {
                if (state.Board[x, y] == enemy)
                    EnemyPawn = true;
                x += i;
                y += j;
            }

            if (!CheckIfInSideBoard((x, y)))
                return (true, false, EnemyPawn);

            return (false, true, EnemyPawn);
        }

        /// <summary>
        /// Oblicza liczbę możliwych ruchów gracza z danej pozycji pionka
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="counted"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Sprawdza czy gracz może położyć pion w kierunku od danej pozycji
        /// </summary>
        /// <param name="state"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="counted"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Sprawdza czy pozycja jest na planszy czy nie
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool CheckIfInSideBoard((int x, int y) position)
        {
            return position.x >= 0 && position.x < Constants.N && position.y >= 0 && position.y < Constants.N;
        }
    }
}
