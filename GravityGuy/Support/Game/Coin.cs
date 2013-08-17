using System.ComponentModel;
using System.Threading.Tasks;
using GravityGuy.Support.Concurrency;
using GravityGuy.Support.Geometry;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Coin game entity.
    /// </summary>
    public class Coin : Actor
    {
        private static readonly PropertyChangedEventArgs AvailabilityProperty = new PropertyChangedEventArgs("Availability");

        /// <summary>
        /// Initializes a new Coin instance.
        /// </summary>
        /// <param name="position">Coin location.</param>
        /// <param name="uiTaskScheduler">Scheduler associated with the UI thread.</param>
        public Coin(Circle position, TaskScheduler uiTaskScheduler)
            : base(uiTaskScheduler)
        {
            this.Position = position;
            this.Availability = CoinAvailability.Free;
        }

        /// <summary>
        /// Gets a value describing the region occupied by this coin. This property is immutable.
        /// </summary>
        public Circle Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value describing the coins availability information.
        /// </summary>
        public CoinAvailability Availability
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a clone of the original coin. The cloned coin is marked as available.
        /// </summary>
        /// <param name="coin">Source coin.</param>
        /// <returns>Cloned coin.</returns>
        public static Coin Reset(Coin coin)
        {
            return new Coin(coin.Position, coin.TaskScheduler);
        }

        /// <summary>
        /// Marks the coin as captured.
        /// </summary>
        /// <returns>A task whose result indicates whether the coin was captured by this caller.</returns>
        public Task<bool> Capture()
        {
            return this.ScheduleOperation(this.DoCapture)
                .ContinueWith<bool>(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        #region Actor Support Code

        /// <summary>
        /// Exclusively performs the actual capture operation.
        /// </summary>
        /// <returns>A value indicating whether the coin was captured by this call.</returns>
        private Task<ActorResponse<bool>> DoCapture()
        {
            if (this.Availability == CoinAvailability.Free)
            {
                this.Availability = CoinAvailability.Captured;
                this.PropertyChanged(AvailabilityProperty);

                return Task.FromResult(new ActorResponse<bool>() { Value = true });
            }

            return Task.FromResult(new ActorResponse<bool>() { Value = false });
        }

        #endregion
    }
}