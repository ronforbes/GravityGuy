using GravityGuy.Support.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GravityGuy
{
    internal partial class Game
    {
        int coins;

        void OnStart()
        {
            // TODO: Reset game state at the start of the game
        }

        void OnCoinCaptured()
        {
            // TODO: Increment coinDisplay and call the UpdateCoinDisplay function to update the number of coins captured
            //UpdateCoinDisplay();

            // TODO: Create a way to detect that the player has won, and then call...
            //Victory();
        }

        void OnVictory()
        {
            // TODO: Set the background of the screen to a different color to indicate victory!
            //VictoryBrush = Brushes.Red;
        }
    }
}
