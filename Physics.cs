using Game.Math;
using System.Collections.Generic;
using System;

namespace Game.Physics {
    public interface IPhysicsShape {
        Vector2 Position { get; set; }

        IEnumerable<Vector2> CastRay(Ray ray);

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

    public class Circle : IPhysicsShape {
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

        ///<summary>
        ///     We calculate the points of interection between the ray
        ///     and the circle by substituting the paramaterised equations
        ///     of a line (the ray) into the equation of a circle:
        ///
        ///     x(t) = ray.origin.X + t * ray.Direction.X
        ///
        ///     y(t) = ray.Origin.Y + t * ray.Direction.Y
        ///
        ///     (x(t) - this.Position.X)^2 + (y(t) - this.Position.Y)^2 = this.Radius^2
        ///
        ///     The resulting equation happens to be quadratic, so we solve
        ///     it for t to get our two intersections.
        ///     Two intersections when the ray goes straight through,
        ///     One intersections when the ray only tangentially touches,
        ///     and none when it doesn't intersect at all. 
        ///     This gives us any intersections along the line given by
        ///     the ray - in both directions - so we cull any intersections
        ///    with a negative t.
        ///</summary>
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

        public void Translate(Vector2 translation) {
            this.centre += translation;
        }

        public void Rotate(double rotation) {
            ///¯\_(ツ)_/¯
        }
    }

    public class Polygon : IPhysicsShape {
        protected Vector2[] points;
        protected Vector2 position;

        public Vector2 Position {
            get => this.position;
            set => this.position = value;
        }

        public Polygon(Vector2 position, IEnumerable<Vector2> points) {
            this.setPoints(points);
            this.position = position;
        }

        protected void setPoints(IEnumerable<Vector2> points) {
            List<Vector2> pointsList = new List<Vector2>();
            foreach(Vector2 point in points) {
                pointsList.Add(new Vector2(point.X, point.Y));
            }

            this.points = pointsList.ToArray();
        }

        public IEnumerable<Vector2> CastRay(Ray ray) {
            throw new NotImplementedException();
        }

        public void Translate(Vector2 translation) {
            this.position += translation;
        }

        public void Rotate(double rotation) {
            Matrix2 rotationMatrix = Matrix2.RotationMatrix(rotation);
            for(int i = 0; i < this.points.Length; i++) {
                this.points[i] = rotationMatrix * this.points[i];
            }
        }

        public void PrintPoints() {
            foreach(Vector2 point in this.points) {
                Console.WriteLine(point);
            }
        }
    }

    public class Rectangle : Polygon {

        private double width;
        private double height;

        public double Width {
            get => this.width;
            set {
                this.width = value;

                var points = getPoints(this.width, this.height);
                this.setPoints(points);
            }
        }

        public double Height {
            get => this.height;
            set {
                this.height = value;

                var points = getPoints(this.width, this.height);
                this.setPoints(points);
            }
        }

        public Rectangle(Vector2 position, double width, double height) 
                : base(position, getPoints(width, height)) {
            this.width = width;
            this.height = height;
        }

        private static IEnumerable<Vector2> getPoints(double width, double height) {
            double halfWidth = width / 2;
            double halfHeight = height / 2;
            return new Vector2[] {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(-halfWidth,  halfHeight),
                new Vector2( halfWidth, -halfHeight),
                new Vector2( halfWidth,  halfHeight)
            };
        }

        public static Rectangle Square(Vector2 centre, double sideLength) =>
            new Rectangle(centre, sideLength, sideLength);

        public static Rectangle FromTopRight(Vector2 position, double width, double height) {
            Vector2 centre = position + new Vector2(width / 2, height / 2);
            return new Rectangle(centre, width, height);
        }

        public static Rectangle SquareFromTopRight(Vector2 position, double sideLength) =>
            FromTopRight(position, sideLength, sideLength);
    }

    public class Line : IPhysicsShape {
        private Vector2 start;
        private Vector2 end;

        public Vector2 Position { 
            get => this.Start; 
            set => this.Start = value; 
        }

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

        public IEnumerable<Vector2> CastRay(Ray ray) {
            Vector2 delta = this.End - this.Start;
            double m = delta.Y / delta.X;
            double c = this.Start.Y - m * this.start.X;

            if(MathHelper.ApproximatelyEqual(ray.Direction.Y, m * ray.Direction.X)) {
                yield break;
            }

            double t = (m * ray.Origin.X + c - ray.Origin.Y) / (ray.Direction.Y - m * ray.Direction.X);

            Vector2 intersection = t * ray.Direction;

            if(t < 0 || (intersection - this.Start).MagnitudeSquared > this.LengthSquared) {
                yield break;
            }

            yield return intersection;
        }

        public void Rotate(double rotation) {
            Vector2 delta = this.End - this.Start;
            Matrix2 rotationMatrix = Matrix2.RotationMatrix(rotation);
            Vector2 rotatedDelta = rotationMatrix * delta;
            this.End = this.Start + rotatedDelta;
        }

        public void Translate(Vector2 translation) {
            this.Start += translation;
        }
    }
}
