using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            GameState EndGameState;
            bool HumanPlayer;
            bool DiffrentHeurestics = false;
            bool OnlyResult = false;

            var res = PickGameModePrompt();

            Heuristics.HeuristicFunc[] PlayersHeuristics; // = new Heuristics.HeuristicFunc[3];

            while (true)
            {
                if (res == "1")
                    HumanPlayer = true;
                else if (res == "2")
                    HumanPlayer = false;
                else if (res == "3")
                {
                    HumanPlayer = false;
                    DiffrentHeurestics = true;
                }
                else if (res == "21")
                {
                    HumanPlayer = false;
                    OnlyResult = true;
                }
                else if (res == "31")
                {
                    HumanPlayer = false;
                    OnlyResult = true;
                    DiffrentHeurestics = true;
                }
                else
                {
                    res = PickGameModePrompt();
                    continue;
                }
                break;
            }

            if (DiffrentHeurestics)
            {
                PlayersHeuristics = new Heuristics.HeuristicFunc[3] {
                    (new Heuristics(0.8, 0.1, 0.1)).Combined,
                    (new Heuristics(0.1, 0.8, 0.1)).Combined,
                    (new Heuristics(0.1, 0.1, 0.8)).Combined,
                };
            }
            else
            {
                PlayersHeuristics = new Heuristics.HeuristicFunc[3] {
                    (new Heuristics(0.33, 0.33, 0.33)).Combined,
                    (new Heuristics(0.33, 0.33, 0.33)).Combined,
                    (new Heuristics(0.33, 0.33, 0.33)).Combined,
                };
            }

            EndGameState = Play(HumanPlayer, PlayersHeuristics, OnlyResult);

            if (OnlyResult)
                EndGameState.Display();

            (int blacks, int whites, int reds) = EndGameState.CountPlayersPawns();
            if (blacks >= whites && blacks >= reds)
                Console.WriteLine("Czarni wygrali");
            if (whites >= blacks && whites >= reds)
                Console.WriteLine("Biali wygrali");
            if (reds >= whites && reds >= blacks)
                Console.WriteLine("Czerwoni wygrali");

            Console.WriteLine($"Czarni:{blacks} , Biali:{whites} , Czerwoni:{reds}");

            Console.WriteLine("Koniec rozgrywki");
        }

        private string PickGameModePrompt()
        {
            Console.WriteLine("Wybierz tryb rozgrywki:\n" +
                "1 - jeżeli chcesz uczestniczyć w grze\n" +
                "2 - jeżeli chcesz oglądać rozgrywkę między AI\n" +
                "3 - jeżeli chcesz rozgrywkę AI z różnymi heurestykami\n" +
                "*1 - jeśli chcesz zobaczyć tylko wynik (niedziała dla pierwszej opcji)");
            return Console.ReadLine()!;
        }

        public void RunAnalysys()
        {
            List<Heuristics.HeuristicFunc> heuristics = new List<Heuristics.HeuristicFunc>() { Heuristics.PawnCount, Heuristics.Mobility, Heuristics.Stability };
            Dictionary<Heuristics.HeuristicFunc, string> heuristicName = new Dictionary<Heuristics.HeuristicFunc, string>()
            {
                { Heuristics.PawnCount, "PawnCount" },
                { Heuristics.Stability, "Stability" },
                { Heuristics.Mobility, "Mobility" },
            };
            Dictionary<Heuristics.HeuristicFunc, int> pawnsCaptured = new Dictionary<Heuristics.HeuristicFunc, int>()
            {
                { Heuristics.PawnCount, 0 },
                { Heuristics.Mobility, 0 },
                { Heuristics.Stability, 0 },
            };
            Dictionary<Heuristics.HeuristicFunc, int> pawnsLost = new Dictionary<Heuristics.HeuristicFunc, int>()
            {
                { Heuristics.PawnCount, 0 },
                { Heuristics.Mobility, 0 },
                { Heuristics.Stability, 0 },
            };
            Dictionary<Heuristics.HeuristicFunc, int> totalWins = new Dictionary<Heuristics.HeuristicFunc, int>()
            {
                { Heuristics.PawnCount, 0 },
                { Heuristics.Mobility, 0 },
                { Heuristics.Stability, 0 },
            };

            foreach (var player1Heuristics in heuristics)
            {
                foreach (var player2Heuristics in heuristics)
                {
                    foreach (var player3Heuristics in heuristics)
                    {
                        var playersHeuristics = new Heuristics.HeuristicFunc[3]
                        {
                            player1Heuristics,
                            player2Heuristics,
                            player3Heuristics,
                        };

                        var EndGameState = Play(false, playersHeuristics, true);

                        (int blacks, int whites, int reds) = EndGameState.CountPlayersPawns();

                        pawnsCaptured[player1Heuristics] += blacks;
                        pawnsCaptured[player2Heuristics] += whites;
                        pawnsCaptured[player3Heuristics] += reds;

                        pawnsLost[player1Heuristics] += whites + reds;
                        pawnsLost[player2Heuristics] += blacks + reds;
                        pawnsLost[player3Heuristics] += blacks + whites;

                        if (blacks >= whites && blacks >= reds)
                            totalWins[player1Heuristics]++;
                        if (whites >= blacks && whites >= reds)
                            totalWins[player2Heuristics]++;
                        if (reds >= whites && reds >= blacks)
                            totalWins[player3Heuristics]++;

                        Console.WriteLine($"({heuristicName[player1Heuristics]}, {heuristicName[player2Heuristics]}, {heuristicName[player3Heuristics]}) = ({blacks}, {whites}, {reds})");
                        currentGameState = GameState.GenerateStart();
                    }
                }
            }

            foreach (var h in heuristics)
            {
                Console.WriteLine($"Pawn's balance of {heuristicName[h]}: total {pawnsCaptured[h]} captured, total {pawnsLost[h]} lost");
            }
            foreach (var h in heuristics)
            {
                Console.WriteLine($"Total number of {heuristicName[h]} wins: {totalWins[h]}");
            }
        }

        public GameState Play(bool IfHumanPlayer, Heuristics.HeuristicFunc[] PlayersHeurictics, bool OnlyResult)
        {
            if(IfHumanPlayer)
                Console.WriteLine("Grasz czarnymi, żeby wybrać pole napisz wartość w typie RC gdzie R to numer wiersz a C to numer kolumny");
            int i = 1;
            int playersWhoCanMove = 3;
            int prevX = 0; int prevY = 0;
            int prevPlayer = 1;

            while (playersWhoCanMove > 0)
            {
                if (!OnlyResult)
                {
                    Console.WriteLine($"Tura {i++}:");

                    if (i > 2)
                    {
                        if (prevX > -1 && prevY > -1)
                            Console.WriteLine($"Gracz {Constants.PickPlayer(prevPlayer)} położył piona na ({prevX},{prevY})");
                        else
                            Console.WriteLine($"Gracz {Constants.PickPlayer(prevPlayer)} musiał ominąć turę");
                    }

                    currentGameState.Display();
                }
                prevPlayer = currentGameState.CurrentPlayer;

                GameState? nextGameState = null;

                if (!IfHumanPlayer || currentGameState.CurrentPlayer != 1)
                {
                    nextGameState = Algorithms.MaxN(currentGameState, PlayersHeurictics);

                    // Jeżeli CurrentPlayer w obecnym stanie rozgrywki nie może wykonać żadnego ruchu
                    if (currentGameState == nextGameState)
                    {
                        prevX = prevY = -1;
                        currentGameState.SkipTurn();
                        playersWhoCanMove--;
                    }
                    else
                    {
                        (prevX, prevY) = currentGameState.FindNextMove(nextGameState);
                        currentGameState = nextGameState;
                        playersWhoCanMove = 3;
                    }
                    if(!OnlyResult)
                        Console.ReadLine();
                }
                else if (currentGameState.CurrentPlayer == 1 && currentGameState.CheckIfCurrentPlayerCanMove())
                {
                    nextGameState = null;
                    while (nextGameState == null)
                    {
                        var res = Console.ReadLine();
                        if (res == null || res.Length != 2)
                        {
                            Console.WriteLine("Błędna długość argumentu\n");
                            continue;
                        }
                        prevX = res[0] - '0';
                        prevY = res[1] - '0';
                        nextGameState = currentGameState.PlayerTryToPlacePawn((prevX, prevY));
                        if (nextGameState == null)
                            Console.WriteLine("Błędny argument\n");
                    }
                    playersWhoCanMove = 3;
                    currentGameState = nextGameState;
                }
                else if (currentGameState.CurrentPlayer == 1)
                {
                    prevX = prevY = -1;
                    currentGameState.SkipTurn();
                    playersWhoCanMove--;
                }
            }

            return currentGameState;
        }
    }
}
