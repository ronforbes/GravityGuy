using System.Windows;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Enumeration of gravity directions.
    /// </summary>
    public enum GravityDirection
    {
        Up,
        Down,
    }

    /// <summary>
    /// Stateful gravity state enumeration.
    /// </summary>
    public sealed class CharacterGravityState
    {
        /// <summary>
        /// Gets the up gravity state.
        /// </summary>
        public static CharacterGravityState Up
        {
            get
            {
                return new CharacterGravityState() { Acceleration = new Vector(0, 5), Direction = GravityDirection.Up };
            }
        }

        /// <summary>
        /// Gets the down gravity state.
        /// </summary>
        public static CharacterGravityState Down 
        {
            get
            {
                return new CharacterGravityState() { Acceleration = new Vector(0, -5), Direction = GravityDirection.Down };
            }
        }

        /// <summary>
        /// Initializes a new CharacterGravityState instance.
        /// </summary>
        private CharacterGravityState()
        {
        }

        /// <summary>
        /// Gets the gravity direction.
        /// </summary>
        public GravityDirection Direction
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the acceleration associated with gravity.
        /// </summary>
        public Vector Acceleration
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the character is grounded.
        /// </summary>
        public bool IsGrounded
        {
            get;
            set;
        }
    }
}