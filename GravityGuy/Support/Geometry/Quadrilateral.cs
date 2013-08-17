using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GravityGuy.Support.Geometry
{
    /// <summary>
    /// This is really just a rectangle, that name is already taken though.
    /// </summary>
    public struct Quadrilateral
    {
        public Quadrilateral(Point corner, Vector diagonal)
        {
            this.Corner = corner;
            this.Diagonal = diagonal;
            this.Region = new Rect(corner, diagonal);
        }

        public Quadrilateral(double cx, double cy, double dx, double dy)
            : this(new Point(cx, cy), new Vector(dx, dy))
        {
        }

        /// <summary>
        /// Gets the lower left corner of the rectangle.
        /// </summary>
        public Point Corner;

        /// <summary>
        /// Gets the diagonal pointing from the lower left corner to the upper right corner of the rectangle.
        /// </summary>
        public Vector Diagonal;

        /// <summary>
        /// Gets the rect region associated with the quad.
        /// </summary>
        public Rect Region;
    }
}
