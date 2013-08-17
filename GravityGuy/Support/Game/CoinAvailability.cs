namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Stateful enumeration used to represent coin availability.
    /// </summary>
    public sealed class CoinAvailability
    {
        /// <summary>
        /// Gets an availability state representing a free coin.
        /// </summary>
        public static readonly CoinAvailability Free = new CoinAvailability() { CanCapture = true };

        /// <summary>
        /// Gets an availability state representing a captured coin.
        /// </summary>
        public static readonly CoinAvailability Captured = new CoinAvailability() { CanCapture = false };

        /// <summary>
        /// Initializes a new CoinAvailability instance.
        /// </summary>
        private CoinAvailability()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the coin is capturable.
        /// </summary>
        public bool CanCapture
        {
            get;
            private set;
        }
    }
}