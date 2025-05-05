using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Utils;

namespace reversi_3_player.Domain
{
    /// <summary>
    /// Klasa reprezentująca stan rozgrywki w grze Reversi utożsamiana z wierzchołkiem grafu tej gry
    /// </summary>
    public class GameState
    {
        public int[,] Board { get; } = new int[Constants.N, Constants.N]; // Plansza w danym stanie rozgrywki
        public List<GameState> Children { get; } = new List<GameState>(); // Stany-dzieci, które są generowane z obecnego stanu
        public int CurrentPlayer { get; private set; } // Gracz, który wykonuje ruch w obecnym stanie
        private List<(int, int)> PawnCoordinates; // Współrzędne wszystkich pionków gracza wykonującego ruch w obecnym stanie

        //private bool WithPlayer = false; // Flaga sygnalizująca czy rozgrywka odbywa się z graczem czy nie

        public GameState(int[,] board, int currentPlayer, List<(int, int)> pawnCoordinates)
        {
            this.Board = board;
            this.CurrentPlayer = currentPlayer;
            this.PawnCoordinates = pawnCoordinates;
        }

        /// <summary>
        /// Generuje początkowy stan rozgrywki
        /// </summary>
        public static GameState GenerateStart()
        {
            int[,] board = new int[Constants.N, Constants.N];

            // Pionki czarne
            board[3, 4] = 1;
            board[4, 5] = 1;
            board[5, 3] = 1;

            // Pionki białe
            board[3, 3] = 2;
            board[4, 4] = 2;
            board[5, 5] = 2;

            // Pionki czerwone
            board[3, 5] = 3;
            board[4, 3] = 3;
            board[5, 4] = 3;

            int currentPlayer = 1;

            // Współrzędne pionków gracza, który rozpoczyna rozgrywkę (czarny)
            List<(int x, int y)> pawnCoordinates = new List<(int, int)>() { (3, 4), (4, 5), (5, 3) };

            return new GameState(board, currentPlayer, pawnCoordinates);
        }

        /// <summary>
        /// Buduje drzewo gry zakorzenione w obecnym stanie
        /// </summary>
        /// <param name="currentDepth">
        /// Aktualna głębokość drzewa
        /// </param>
        public void BuildGameTree(int currentDepth)
        {
            if (currentDepth > Constants.Depth)
            {
                return;
            }
            else
            {
                if (GenerateChildren())
                {
                    foreach (var child in Children)
                    {
                        child.BuildGameTree(currentDepth + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Czyści drzewo gry zakorzenione w obecnym stanie
        /// </summary>
        public void ClearGameTree()
        {
            if (Children.Count == 0)
            {
                return;
            }
            else
            {
                foreach (var child in Children)
                {
                    child.ClearGameTree();
                }
                Children.Clear();
            }
        }

        /// <summary>
        /// Generuje wszystkie stany-dzieci stanu obecnego
        /// </summary>
        /// <returns>
        /// true jeżeli można wygenerować przynajmniej jedno dziecko, false w.p.p
        /// </returns>
        private bool GenerateChildren()
        {
            // Lista wszystkich kierunków, w których można dostawić pionka wykonując poprawny ruch
            List<(int, int)> directions = new List<(int, int)>()
            {
                (0, -1), // lewo
                (0, 1), // prawo
                (-1, 0), // góra
                (1, 0), // dół
                (-1, -1), // lewo-góra
                (-1, 1), // prawo-góra
                (1, 1), // prawo-dół
                (1, -1) // lewo-dół
            };

            // Tablica, w której zapisujemy dostawiane pionki, oraz pola "przejmowane" przez
            // CurrentPlayera wraz z jego dostawieniem. Jeżeli pawnsPlaced[i, j] != null, to znaczy,
            // że w miejscu (i, j) poprawnie dostawiono pionek generując ruch gracza CurrentPlayer,
            // w którym zdobył on pionki przeciwników z pól znajdujących się w liście pawnsPlaced[i, j]
            List<(int, int)>[,] pawnsPlaced = new List<(int, int)>[Constants.N, Constants.N];

            // Dla każdego pionka gracza CurrentPlayer znajdującego się obecnie na planszy
            // dostawiamy nowy pionek we wszystkich możliwych miejscach gwarantujących poprawny ruch.
            // W ten sposób generujemy wszystkie możliwe ruchy jakie może wykonać gracz CurrentPlayer
            // w obecnym stanie rozgrywki
            foreach (var startPawn in PawnCoordinates) 
            {
                foreach (var direction in directions)
                {
                    TryPlacePawnInDirection(startPawn, direction, pawnsPlaced);
                }
            }

            bool IsAbleToPlacePawn = false;

            for (int i = 0; i < Constants.N; i++) 
            {
                for (int j = 0; j < Constants.N; j++) 
                {
                    if (pawnsPlaced[i, j] != null)
                    {
                        // Oznaczamy, że z obecnego stanu da się wykonać jakiś ruch
                        IsAbleToPlacePawn = true;

                        // Dodajemy pole, na którym dostawiliśmy pionek do listy pól zdobytych
                        pawnsPlaced[i, j].Add((i, j));

                        // Generujemy stan-dziecko reprezentujący kolejny ruch CurrentPlayera, w którym
                        // dostawia on pionka na polu (i, j)
                        GenerateChild(pawnsPlaced[i, j]);
                    }
                }
            }

            return IsAbleToPlacePawn;
        }

        private void TryPlacePawnInDirection((int x, int y) start, (int x, int y) direction, List<(int, int)>[,] pawnsPlaced)
        {
            // Potencjalne pola (x, y), na których możemy dostawić pionek szukamy zaczynając od pola
            // znajdującego się 2 miejsca w kierunku direction od pola start
            int x = start.x + 2 * direction.x;
            int y = start.y + 2 * direction.y;

            // Sprawdzamy, czy w ogóle potencjalne pole (x, y) mieści się na planszy
            while (IsInsideBoard(x, y))
            {
                // Sprawdzamy, czy potencjalne pole (x, y) jest puste
                if (Board[x, y] == 0)
                {
                    bool isValid = true;
                    List<(int, int)> takenFields = new();

                    // Współrzędne aktualnie przetwarzanego pola "pośredniczącego" znajdującego się pomiędzy
                    // potencjalnym polem (x, y), na którym stawiamy pionek a polem startowym
                    int x_intermediate = x - direction.x;
                    int y_intermediate = y - direction.y;

                    // Przechodzimy przez wszystkie pola pośredniczące pomiędzy (x, y) a start
                    while ((x_intermediate, y_intermediate) != (start.x, start.y))
                    {
                        // Sprawdzamy czy pole pośredniczące jest "złe" (jest puste lub jest na nim pionek koloru CurrentPlayer)
                        // i jeżeli tak to przerywamy
                        if (Board[x_intermediate, y_intermediate] == 0 || Board[x_intermediate, y_intermediate] == CurrentPlayer)
                        {
                            isValid = false;
                            break;
                        }

                        // Jeżeli pole jest "dobre" to dodajemy je do listy pól "zdobytych" przez CurrentPlayera
                        // po dostawieniu pionka (x, y)
                        takenFields.Add((x_intermediate, y_intermediate));

                        x_intermediate -= direction.x;
                        y_intermediate -= direction.y;
                    }

                    // Jeżeli pomiędzy nie było "złych" pól to stawiamy pionek na polu (x, y)
                    if (isValid)
                    {
                        // Jeżeli w żadnym z poprzednich wygenerowanych stanów-dzieci pionek nie był
                        // stawiany na polu (x, y)
                        if (pawnsPlaced[x, y] == null)
                        {
                            pawnsPlaced[x, y] = takenFields;
                        }
                        else // Jeżeli był stawiany
                        {
                            foreach(var takenField in takenFields)
                            {
                                pawnsPlaced[x, y].Add(takenField);
                            }
                        }
                    }

                    // Analizujemy tylko pierwsze puste pole. Jeżeli nie udało się postawić na nim pionka,
                    // to na kolejnych pustych polach w kierunku direction też nie będzie się dało
                    break;
                }

                // Sprawdzamy kolejne pole w kierunku direction jeżeli aktulane okazało się niepuste
                x += direction.x;
                y += direction.y;
            }
        }

        /// <summary>
        /// Sprawdza czy współrzędne (x, y) mieszczą się w tablicy
        /// </summary>
        private bool IsInsideBoard(int x, int y)
        {
            return x >= 0 && x < Constants.N && y >= 0 && y < Constants.N;
        }

        /// <summary>
        /// Generuje nową planszę na podstawie listy pól zajętych przez podanego gracza
        /// </summary>
        /// <remarks>
        /// Na podstawie podanej planszy board tworzy kopię planszy, w której na polach o współrzędnych
        /// z listy takenFields postawione są pionki gracza player
        /// </remarks>
        private int[,] GenerateNewBoard(int[,] board, List<(int x, int y)> takenFields, int player)
        {
            int N = board.GetLength(0);
            int[,] childBoard = new int[N, N];

            for (int i = 0; i < N; i++) 
            {
                for (int j = 0; j < N; j++) 
                {
                    childBoard[i, j] = board[i, j];
                }
            }

            foreach(var field in takenFields)
            {
                childBoard[field.x, field.y] = player;
            }

            return childBoard;
        }

        /// <summary>
        /// Zwraca współrzędne wszystkich pionków gracza player na planszy board
        /// </summary>
        private List<(int x, int y)> GetPawnCoords(int[,] board, int player)
        {
            int N = board.GetLength(0);
            List<(int, int)> pawnCoords = new List<(int, int)>();

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (board[i, j] == player)
                    {
                        pawnCoords.Add((i, j));
                    }
                }
            }

            return pawnCoords;
        }

        private int DetermineNextPlayer()
        {
            return CurrentPlayer == 3 ? 1 : CurrentPlayer + 1;
        }

        /// <summary>
        /// Generuje stan-dziecko obecnego stanu będące stanem rozgrywki po wykonaniu ruchu przez CurrentPlayera
        /// </summary>
        private void GenerateChild(List<(int, int)> takenFields)
        {
            int[,] newBoard = GenerateNewBoard(Board, takenFields, CurrentPlayer);
            int nextPlayer = DetermineNextPlayer();
            List<(int, int)> nextPlayerPawnCoords = GetPawnCoords(newBoard, nextPlayer);

            Children.Add(new GameState(newBoard, nextPlayer, nextPlayerPawnCoords));
        }

        public void SkipTurn()
        {
            CurrentPlayer = DetermineNextPlayer();
            PawnCoordinates = GetPawnCoords(Board, CurrentPlayer);
        }

        /// <summary>
        /// Wyświetla stan rozgrywki na konsoli
        /// </summary>
        public void Display()
        {
            var boardBackgroundColor = ConsoleColor.Green;

            // Numeracja kolumn
            Console.Write("   ");
            for (int i = 0; i < Constants.N; i++)
            {
                Console.Write($" {i}");
            }
            Console.WriteLine();

            for (int i = 0; i < Constants.N; i++)
            {
                // Numeracja wierszy
                Console.Write($" {i} ");

                // Rysowanie tablicy
                for (int j = 0; j < Constants.N; j++)
                {
                    if (Board[i, j] == 0) // Pole bez pionka
                    {
                        ConsoleUtils.WriteColored("| ", ConsoleColor.Black, boardBackgroundColor);
                    }
                    else // Pole z pionkiem
                    {
                        ConsoleUtils.WriteColored("|", ConsoleColor.Black, boardBackgroundColor);
                        ConsoleColor foregroundColor;
                        if (Board[i, j] == 1)
                        {
                            foregroundColor = ConsoleColor.Black;
                        }
                        else if (Board[i, j] == 2)
                        {
                            foregroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            foregroundColor = ConsoleColor.Red;
                        }
                        ConsoleUtils.WriteColored("O", foregroundColor, boardBackgroundColor);
                    }
                }

                ConsoleUtils.WriteColored("|", ConsoleColor.Black, boardBackgroundColor);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Próbuje położyć pion na danej pozycji według gracza
        /// </summary>

        public GameState? PlayerTryToPlacePawn((int x, int y) Position)
        {
            if (!IsInsideBoard(Position.x, Position.y) || CurrentPlayer != 1 || Board[Position.x, Position.y] != 0)
                return null;

            List<(int, int)> directions = new List<(int, int)>()
            {
                (0, -1), // lewo
                (0, 1), // prawo
                (-1, 0), // góra
                (1, 0), // dół
                (-1, -1), // lewo-góra
                (-1, 1), // prawo-góra
                (1, 1), // prawo-dół
                (1, -1) // lewo-dół
            };

            var valid = new bool[8];
            var endPos = new (int x, int y)[8];
            int n = 0;
            foreach(var direction in directions)
            {
                (valid[n], endPos[n]) = CheckFromDirection(Position, (direction.Item1, direction.Item2));
                n++;
            }

            var takenFields = new List<(int x, int y)>() { Position };

            for (int k = 0;k < n;k++)
            {
                if (valid[k])
                {
                    (int x, int y) = Position;
                    (int i, int j) = directions.ElementAt(k);
                    do
                    {
                        x += i;
                        y += j;
                        takenFields.Add((x, y));
                    }
                    while (x != endPos[k].x || y != endPos[k].y);
                }
            }

            if (takenFields.Count == 1)
                return null;

            int[,] newBoard = GenerateNewBoard(Board, takenFields, CurrentPlayer);
            int nextPlayer = 2;
            List<(int, int)> nextPlayerPawnCoords = GetPawnCoords(newBoard, nextPlayer);

            return new GameState(newBoard, nextPlayer, nextPlayerPawnCoords);
        }

        /// <summary>
        /// Sprawdza czy z danego kierunku do danej pozycji można położyć pion gracza
        /// </summary>
        private (bool flag, (int, int) endPos) CheckFromDirection((int, int) Position, (int,int) Direction)
        {
            (int x, int y) = Position;
            (int i, int j) = Direction;
            x += i;
            y += j;
            if (IsInsideBoard(x, y) && (Board[x, y] == 0 || Board[x, y] == CurrentPlayer))
                return (false, (-1, -1));
            x += i;
            y += j;
            while (IsInsideBoard(x, y) && Board[x, y] != 0)
            {
                if (Board[x, y] == CurrentPlayer)
                    return (true, (x,y));
                x += i;
                y += j;
            }
            return (false , (-1, -1));
        }

        /// <summary>
        /// Sprawdza czy w danym kierunku z danej pozycji można położyć pion gracza
        /// </summary>
        private (bool flag, (int, int) endPos) CheckToDirection((int, int) Position, (int, int) Direction)
        {
            (int x, int y) = Position;
            (int i, int j) = Direction;
            x += i;
            y += j;
            if (IsInsideBoard(x, y) && (Board[x, y] == 0 || Board[x, y] == CurrentPlayer))
                return (false, (-1, -1));
            x += i;
            y += j;
            while (IsInsideBoard(x, y) && Board[x, y] != CurrentPlayer)
            {
                if (Board[x, y] == 0)
                    return (true, (x, y));
                x += i;
                y += j;
            }
            return (false, (-1, -1));
        }

        public bool CheckIfCurrentPlayerCanMove()
        {
            var Pawns = GetPawnCoords(Board, CurrentPlayer);
            if (Pawns.Count == 0)
                return false;

            List<(int, int)> directions = new List<(int, int)>()
            {
                (0, -1), // lewo
                (0, 1), // prawo
                (-1, 0), // góra
                (1, 0), // dół
                (-1, -1), // lewo-góra
                (-1, 1), // prawo-góra
                (1, 1), // prawo-dół
                (1, -1) // lewo-dół
            };

            foreach (var pawn in Pawns)
            {
                foreach(var direction in directions)
                {
                    if (CheckToDirection(pawn, direction).flag)
                        return true;
                }
            }

            return false;
        }

        public (int blacks, int whites, int reds) CountPlayersPawns()
        {
            (int blacks, int whites, int reds) = (0, 0, 0);
            for (int i = 0;i < Constants.N;i++)
            {
                for (int j = 0; j < Constants.N; j++)
                {
                    switch(Board[i,j])
                    {
                        case 1:
                            blacks++;
                            break;
                        case 2:
                            whites++;
                            break;
                        case 3:
                            reds++;
                            break;
                    }
                }
            }
            return (blacks, whites, reds);
        }

        public (int, int) FindNextMove(GameState AfterState)
        {
            for (int i = 0; i < Constants.N; i++)
            {
                for (int j = 0; j < Constants.N; j++)
                {
                    if (Board[i, j] == 0 && AfterState.Board[i, j] == CurrentPlayer)
                        return (i, j);
                }
            }
            return (-1, -1);
        }
    }
}
