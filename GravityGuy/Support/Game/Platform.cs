using GravityGuy.Support.Geometry;

namespace GravityGuy.Support.Game
{
    /// <summary>
    /// Describes the position of a platform.
    /// </summary>
    public sealed class Platform
    {
        /// <summary>
        /// Initializes a new Platform instance.
        /// </summary>
        /// <param name="orientation">Platform orientation information.</param>
        /// <param name="position">Players rectangular positions.</param>
        public Platform(PlatformOrientation orientation, Quadrilateral position)
        {
            this.Orientation = orientation;
            this.Position = position;
        }

        /// <summary>
        /// Gets a value describing the region occupied by this platform. This property is immutable.
        /// </summary>
        public Quadrilateral Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value describing the orientation characteristics of this platform. This property is immutable.
        /// </summary>
        public PlatformOrientation Orientation
        {
            get;
            private set;
        }
    }
}