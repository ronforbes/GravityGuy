using System.Windows.Media;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Stateful game state enumeration.
    /// </summary>
    public class GameRunState
    {
        /// <summary>
        /// Gets state representing a game that has not run yet.
        /// </summary>
        public static readonly GameRunState NotStarted = new GameRunState() { Background = Brushes.Azure };

        /// <summary>
        /// Gets state representing a running game.
        /// </summary>
        public static readonly GameRunState Running = new GameRunState() { Background = Brushes.CornflowerBlue };

        /// <summary>
        /// Gets state representing a paused game.
        /// </summary>
        public static readonly GameRunState Paused = new GameRunState() { Background = Brushes.Gray };

        /// <summary>
        /// Gets state representing a game over game.
        /// </summary>
        public static readonly GameRunState GameOver = new GameRunState() { Background = Brushes.Black };

        /// <summary>
        /// Gets the background brush associated with this run state.
        /// </summary>
        public Brush Background
        {
            get;
            set;
        }
    }
}