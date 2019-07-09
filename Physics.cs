using Game.Math;
using System.Collections.Generic;
using System;

namespace Game.Physics {
    public interface IPhysicsBody {
        Vector2 Position {
            get;
            set;
        }

        IEnumerable<Vector2> CastRay(Ray ray);

        void Transform(double scale);

        void Translate(Vector2 translation);

        void Rotate(double rotation);
    }

    public class Ray {
        public Vector2 Origin;
        public Vector2 Direction;

        public Ray(Vector2 origin, Vector2 direction) {
            this.Origin = origin;
            this.Direction = direction;
        }
    }

    public class Circle : IPhysicsBody {
        private Vector2 centre;

        public Vector2 Position {
            get => this.centre;
            set => this.centre = value;
        }
        public double Radius;

        public Circle(Vector2 centre, double radius) {
            this.centre = centre;
            this.Radius = radius;
        }

        // 
        // Summary:
        //      We calculate the points of interection between the ray
        //      and the circle by substituting the paramaterised equations
        //      of a line (the ray) into the equation of a circle. 

        //      x(t) = ray.origin.X + t * ray.Direction.X
        //      y(t) = ray.Origin.Y + t * ray.Direction.Y
        //      (x(t) - this.Position.X)^2 + (y(t) - this.Position.Y)^2 = this.Radius^2
        
        //      The resulting equation happens to be quadratic, so we solve
        //      it for t to get our two intersections.
        //      Two intersections when the ray goes straight through,
        //      One intersections when the ray only tangentially touches,
        //      and none when it doesn't intersect at all. 
        //      This gives us any intersections along the line given by
        //      the ray - in both directions - so we cull any intersections
        //      with a negative t.
        public IEnumerable<Vector2> CastRay(Ray ray) {
            Vector2 delta = ray.Origin - this.Position;

            double a = ray.Direction.MagnitudeSquared;
            double b = 2 * ray.Direction.Dot(delta);
            double c = delta.MagnitudeSquared - this.Radius * this.Radius;

            List<Vector2> intersections = new List<Vector2>();

            if(a < double.Epsilon) {
                return intersections;
            }
            double det = System.Math.Sqrt(b * b - 4 * a * c);

            double t1 = (-b + det) / (2 * a);
            double t2 = (-b - det) / (2 * a);

            if(t1 >= 0) {
                intersections.Add(ray.Origin + t1 * ray.Direction);
            }

            if(!MathHelper.ApproximatelyEqual(t1, t2) && t2 >= 0) {
                intersections.Add(ray.Origin + t2 * ray.Direction);
            }

            return intersections;
        }

        public void Transform(double scale) {
            this.Radius *= scale;
        }

        public void Translate(Vector2 translation) {
            this.centre += translation;
        }

        public void Rotate(double rotation) {
            // ¯\_(ツ)_/¯
        }
    }

    public class Rectangle {
        public Vector2 Centre;
        public double MajorSideLength;
        public double MinorSideLength;

        public Rectangle(Vector2 centre, double majorLength, double minorLength) {
            this.Centre = centre;
            this.MajorSideLength = majorLength;
            this.MinorSideLength = minorLength;
        }

        public Rectangle(Vector2 centre, double sideLength) 
                : this(centre, sideLength, sideLength) {
        }
    }

    public class Line {
        private Vector2 start;
        private Vector2 end;

        private double length;

        private bool isDirty;

        public Line(Vector2 start, Vector2 end) {
            this.start = start;
            this.end = end;
        }

        public Vector2 Start {
            get => this.start;
            set {
                this.start = value;
                this.isDirty = true;
            }
        }

        public Vector2 End {
            get => this.end;
            set {
                this.end = value;
                this.isDirty = true;
            }
        }

        public double LengthSquared {
            get {
                double dX = end.X - start.X;
                double dY = end.Y - start.Y;
                return dX * dX + dY * dY;
            }
        }

        public double Length {
            get {
                if(this.isDirty) {
                    this.length = System.Math.Sqrt(this.LengthSquared);
                    this.isDirty = false;
                }

                return this.length;
            }
        }
    }
}