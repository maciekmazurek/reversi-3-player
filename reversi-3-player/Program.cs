#define ANALYSYS_MODE

using reversi_3_player.UI;

namespace reversi_3_player
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Reversi reversi = new Reversi();
#if ANALYSYS_MODE
            reversi.RunAnalysys();
#else
            reversi.Run();
#endif
        }
    }
}
