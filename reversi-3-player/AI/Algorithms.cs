using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Domain;

namespace reversi_3_player.AI
{
    /// <summary>
    /// Klasa statyczna zawierająca algorytmy
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// Impelemntuje algorytm max^n
        /// </summary>
        /// <param name="rootState">
        /// Korzeń, czyli wierzchołek (stan gry) z którego zaczynamy budowę drzewa gry
        /// </param>
        /// <param name="h">
        /// Heurysytka, za pomocą której obliczamy wartość stanu dla danego gracza
        /// </param>
        /// <returns>
        /// Stan, reprezentujący kolejny ruch 
        /// </returns>
        public static GameState MaxN(GameState rootState, Heuristics.HeuristicFunc h)
        {
            rootState.BuildGameTree(0);
            var (_, nextState) = MaxNRecursive(rootState, h, 0, double.MaxValue);
            rootState.ClearGameTree();

            return nextState;
        }

        /// <summary>
        /// Rekurencyjna metoda pomocnicza dla algorytmu max^n
        /// </summary>
        private static (Dictionary<int, double>, GameState) MaxNRecursive(GameState state, Heuristics.HeuristicFunc h, int currentDepth, double lowerBound)
        {
            // Jeżeli doszliśmy do stanu-liścia zwracamy wartości heurystyki obliczone dla wszystkich graczy i stan
            // związany z tym zestawem wartości
            if (currentDepth == Constants.Depth || state.Children.Count == 0)
            {
                return (CalcHeuristicValsForPruning(state, h), state);
            }
            else
            {
                // Słownik mapujący gracza na jego wartość heurystyki przypisaną w obecnym stanie przez algorytm max^n
                Dictionary<int, double> heuristicValues = new Dictionary<int, double>()
                {
                    {1, double.MinValue},
                    {2, double.MinValue},
                    {3, double.MinValue},
                };
                GameState returnState = null!;

                // Przechodzimy rekurencyjnie po każdym dziecku (DFS)
                foreach (var child in state.Children)
                {
                    var (childValues, childState) = MaxNRecursive(child, h, currentDepth + 1, lowerBound);

                    // Prunujemy gałąź jeżeli wiemy, że na pewno nie zostanie ona wybrana przez gracza znajdującego się "wyżej w drzewie"
                    if (childValues[state.CurrentPlayer] > lowerBound)
                    {
                        break;
                    }

                    // Wybieramy taki zestaw wartości heurystyk, który jest najbardziej korzystny
                    // dla gracza CurrentPlayer
                    if (childValues[state.CurrentPlayer] > heuristicValues[state.CurrentPlayer])
                    {
                        heuristicValues = childValues;

                        // Parametr określający ile najwyżej może wynosić wynik zdobyty przez pozostałych graczy w stanach-dzieciach
                        // obecnego stanu
                        lowerBound = heuristicValues.Sum(x => x.Value) - heuristicValues[state.CurrentPlayer];

                        // Jeżeli jesteśmy w korzeniu to zwracamy naszego potomka będącego
                        // szukanym stanem reprezentującym "kolejny ruch" gracza CurrentPlayer
                        if (currentDepth == 0)
                        {
                            returnState = childState;
                        }
                        else // W pozostałych przypadkach zwracamy samego siebie
                        {
                            returnState = state;
                        }
                    }
                }

                // Zwracamy wartości heurystyk dla wszystkich graczy oraz stan przypisany do tego zestawu wartości
                return (heuristicValues, returnState);
            }
        }

        /// <summary>
        /// Oblicza wartości heurystyk odpowiednio przeskalowując je tak, aby wykonywalny
        /// był shallow-pruning z ich wykorzystaniem
        /// </summary>
        private static Dictionary<int, double> CalcHeuristicValsForPruning(GameState state, Heuristics.HeuristicFunc h)
        {
            // Shiftujemy wartości tak, aby zawsze były nieujemne
            int shift = 100;
            List<double> shiftedValues = new List<double> { h(state, 1) + shift, h(state, 2) + shift, h(state, 3) + shift };
            
            // Normalizujemy wartości tak, aby ich łączna suma zawsze była stała
            double sum = shiftedValues.Sum();
            return new Dictionary<int, double> { 
                { 1, 100 * (shiftedValues[0] / sum) }, 
                { 2, 100 * (shiftedValues[1] / sum) }, 
                { 3, 100 * (shiftedValues[2] / sum) },
            };
        }
    }
}
