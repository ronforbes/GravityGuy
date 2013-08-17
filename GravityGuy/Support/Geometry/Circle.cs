using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GravityGuy.Support.Geometry
{
    public struct Circle
    {
        public Circle(Point center, double radius)
        {
            Vector vc = new Vector(radius / Math.Sqrt(2), radius / Math.Sqrt(2));

            this.Center = center;
            this.Radius = radius;
            this.CollisionRegion = new Rect(center - vc, center + vc);
        }

        public Point    Center;
        public double   Radius;
        public Rect     CollisionRegion;
    }
}
