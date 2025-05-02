using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reversi_3_player.Utils
{
    /// <summary>
    /// Klasa zawierająca metody pomocniczne związane z operacjami konsolowymi
    /// </summary>
    public static class ConsoleUtils
    {
        /// <summary>
        /// Wypisuje pokolorowany text na standard output stream
        /// </summary>
        /// <param name="text">
        /// Tekst wypisywany na standard output stream
        /// </param>
        /// <param name="foregroundColor">
        /// Kolor wypisywanego tekstu
        /// </param>
        /// Kolor tła wypisywanego tekstu
        /// <param name="backgroundColor"></param>
        public static void WriteColored(string text, 
            ConsoleColor foregroundColor = ConsoleColor.White, 
            ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var prevForegroundColor = Console.ForegroundColor;
            var prevBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            Console.Write(text);

            Console.ForegroundColor = prevForegroundColor;
            Console.BackgroundColor = prevBackgroundColor;
        }
    }
}
