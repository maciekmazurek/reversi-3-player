using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reversi_3_player.Domain;

namespace reversi_3_player
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
        /// <param name="WithPlayer">
        /// Flaga, która sygnalizuje czy rozgrywka odbywa się z graczem czy nie
        /// </param>
        /// <returns>
        /// Stan, reprezentujący kolejny ruch 
        /// </returns>
        public static GameState MaxN(GameState rootState, Heuristics.HeuristicFunc h)
        {
            rootState.BuildGameTree(0);
            var (values, nextState) = MaxNRecursive(rootState, h, 0);
            rootState.ClearGameTree();

            return nextState;
        }

        /// <summary>
        /// Rekurencyjna metoda pomocnicza dla algorytmu max^n
        /// </summary>
        private static (Dictionary<int, double>, GameState) MaxNRecursive(GameState state, Heuristics.HeuristicFunc h, int currentDepth)
        {
            // Jeżeli doszliśmy do stanu-liścia zwracamy wartości heurystyki obliczone dla wszystkich graczy i stan
            // związany z tym zestawem wartości
            if (currentDepth == Constants.Depth)
            {
                //if (WithPlayer)
                //    return (new Dictionary<int, double>() { { 2, h(state, 2) }, { 3, h(state, 3) } }, state);
                return (new Dictionary<int, double>() { {1, h(state, 1)}, {2, h(state, 2)}, {3, h(state, 3)} }, state);
            }
            else
            {
                // Słownik mapujący gracza na jego wartość heurystyki przypisaną w obecnym stanie przez algorytm max^n
                Dictionary<int, double> heuristicValues;
                ////if (WithPlayer)
                ////    heuristicValues = new Dictionary<int, double>()
                ////    {
                ////        {2, double.MinValue},
                ////        {3, double.MinValue},
                ////    };
                //else
                heuristicValues = new Dictionary<int, double>()
                {
                    {1, double.MinValue},
                    {2, double.MinValue},
                    {3, double.MinValue},
                };
                GameState returnState = null!;

                // Przechodzimy rekurencyjnie po każdym dziecku (DFS)
                foreach (var child in state.Children)
                {
                    var (childValues, childState) = MaxNRecursive(child, h, currentDepth + 1);

                    // Wybieramy taki zestaw wartości heurystyk, który jest najbardziej korzystny
                    // dla gracza CurrentPlayer
                    if (childValues[state.CurrentPlayer] > heuristicValues[state.CurrentPlayer])
                    {
                        heuristicValues = childValues;

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
    }
}
