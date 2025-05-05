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
            Console.WriteLine("Wybierz tryb rozgrywki:\n" +
                "1 - jeżeli chcesz uczestniczyć w grze\n" +
                "2 - jeżeli chcesz oglądać rozgrywkę między AI");
            var res = Console.ReadLine();

            GameState EndGameState;

            bool flag;

            while (true)
            {
                if (res == "1")
                    flag = true;
                else if (res == "2")
                    flag = false;
                else
                    continue;
                break;
            }

            EndGameState = Play(flag);

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

        public GameState Play(bool IfHumanPlayer)
        {
            if(IfHumanPlayer)
                Console.WriteLine("Grasz czarnymi, żeby wybrać pole napisz wartość w typie RC gdzie R to numer wiersz a C to numer kolumny");
            int i = 1;
            int playersWhoCanMove = 3;
            int prevX = 0; int prevY = 0;
            int prevPlayer = 1;

            while (playersWhoCanMove > 0)
            {
                Console.WriteLine($"Tura {i++}:");

                if (i > 2)
                {
                    if (prevX > -1 && prevY > -1)
                        Console.WriteLine($"Gracz {Constants.PickPlayer(prevPlayer)} położył piona na ({prevX},{prevY})");
                    else
                        Console.WriteLine($"Gracz {Constants.PickPlayer(prevPlayer)} musiał ominąć rundę");
                }

                currentGameState.Display();
                prevPlayer = currentGameState.CurrentPlayer;

                GameState? nextGameState = null;

                if (!IfHumanPlayer || currentGameState.CurrentPlayer != 1)
                {
                    nextGameState = Algorithms.MaxN(currentGameState, Heuristics.Combined);

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
                            Console.WriteLine("Błędny atgument\n");
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
