using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using GravityGuy.Support.Concurrency;
using GravityGuy.Support.Geometry;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Represents a character game entity.
    /// </summary>
    public sealed class Character : Actor
    {
        // Position property information.
        private static readonly PropertyChangedEventArgs PositionProperty = new PropertyChangedEventArgs("Position");

        // Animation state property information.
        private static readonly PropertyChangedEventArgs AnimationStateProperty = new PropertyChangedEventArgs("AnimationState");

        // Cached response associated with synchronous completion - this is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse> CachedResponse = Task.FromResult(new ActorResponse());

        // Cached response associated with TRUE - this is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse<bool>> CachedResponseTrue = Task.FromResult(new ActorResponse<bool>() { Value = true });

        // Cached response associated with FALSE - tihs is used to minimize per-frame heap allocations
        private static readonly Task<ActorResponse<bool>> CachedResponseFalse = Task.FromResult(new ActorResponse<bool>() { Value = false});

        /// <summary>
        /// Initializes a new character instance.
        /// </summary>
        /// <param name="p0">Lower left character position.</param>
        /// <param name="size">Size of the character.</param>
        /// <param name="uiTaskSchedule">Scheduler associated with the UI thread.</param>
        public Character(Point p0, Vector size, TaskScheduler uiTaskSchedule)
            : base(uiTaskSchedule)
        {
            this.Position = new Quadrilateral(p0, size);
            this.Gravity = CharacterGravityState.Down;
            this.Animation = CharacterAnimationState.Down;
        }

        /// <summary>
        /// Gets a value describing the region occupied by this platform.
        /// </summary>
        public Quadrilateral Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current character key frame animation state.
        /// </summary>
        public CharacterAnimationState Animation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current character gravity state.
        /// </summary>
        public CharacterGravityState Gravity
        {
            get;
            private set;
        }

        /// <summary>
        /// Attempts to flip gravity for the character. Gravity flip only succeeds when the
        /// character is grounded.
        /// </summary>
        /// <returns>Task whose result indicates whether the flip occurred.</returns>
        public Task<bool> GravityFlip()
        {
            return this.ScheduleOperation(this.DoGravityFlip)
                .ContinueWith<bool>(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Advances the players position.
        /// </summary>
        /// <param name="positionChange">Describes the players position change.</param>
        /// <returns>Task used to track the completion of this operation.</returns>
        public Task AdvancePosition(PositionChange positionChange)
        {
            return this.ScheduleOperation(this.DoAdvancePosition, positionChange)
                .ContinueWith(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Advances the player animation frame
        /// </summary>
        /// <param name="delta">Frame delta.</param>
        /// <returns>Task used to track the completion of this operation.</returns>
        public Task AdvanceAnimation(double delta)
        {
            return this.ScheduleOperation(this.DoAdvanceAnimation, delta)
                .ContinueWith(this.ExtractResult, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Exclusively performs the gravity flip operation.
        /// </summary>
        /// <returns>A value indicating whether the actual flip occurred.</returns>
        private Task<ActorResponse<bool>> DoGravityFlip()
        {
            if (this.Gravity.IsGrounded)
            {
                if (this.Gravity.Direction == GravityDirection.Up)
                {
                    this.Gravity = CharacterGravityState.Down;
                    this.Animation = CharacterAnimationState.FallingDown;
                }
                else
                {
                    this.Gravity = CharacterGravityState.Up;
                    this.Animation = CharacterAnimationState.FallingUp;
                }

                return CachedResponseTrue;
            }

            return CachedResponseFalse;
        }

        /// <summary>
        /// Exclusively performs the character position change operation.
        /// </summary>
        /// <param name="positionChange">Character position change information.</param>
        /// <returns>Task used to track the operations completion.</returns>
        private Task<ActorResponse> DoAdvancePosition(PositionChange positionChange)
        {
            this.Position = new Quadrilateral(this.Position.Corner + positionChange.Delta, this.Position.Diagonal);
            this.Gravity.IsGrounded = positionChange.Grounded;

            return CachedResponse;
        }

        /// <summary>
        /// Exclusively advances the animation frame forward.
        /// </summary>
        /// <param name="delta">Animation frame change.</param>
        /// <returns>Task used to track the operations completion.</returns>
        public Task<ActorResponse> DoAdvanceAnimation(double delta)
        {
            this.Animation += delta;

            this.PropertyChanged(AnimationStateProperty);

            return CachedResponse;
        }

        // Character position change information.
        public struct PositionChange
        {
            // Characters change in position.
            public Vector   Delta;

            // Indicates whether the character is grounded.
            public bool     Grounded;

            /// <summary>
            /// Creates a new PositionChange instance.
            /// </summary>
            /// <param name="delta">Change in character position.</param>
            /// <param name="grounded">Indicates whether the character is grounded.</param>
            /// <returns>New PositionChange instance.</returns>
            public static PositionChange Create(Vector delta, bool grounded)
            {
                return new PositionChange() { Delta = delta, Grounded = grounded };
            }
        }
    }
}