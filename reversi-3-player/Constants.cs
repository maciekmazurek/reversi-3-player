using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reversi_3_player
{
    /// <summary>
    /// Klasa statyczna zawierająca stałe globalne dla całego programu
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Rozmiar planszy
        /// </summary>
        public const int N = 9;

        /// <summary>
        /// Głębokość budowy drzewa gry
        /// </summary>
        /// <remarks>
        /// Wartość określająca na ile poziomów w dół budowane jest drzewo gry przy każdym ruchu AI oponenta
        /// </remarks>
        public static readonly int Depth = 2;

        public static readonly double weight_p = 0.33;
        public static readonly double weight_m = 0.33;
        public static readonly double weight_s = 0.33;
    }
}
