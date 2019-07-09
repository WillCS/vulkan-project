using System;

namespace Game.Math {

    public class MathHelper {
        public static readonly double EPSILON = 0.001D;
        public static bool ApproximatelyEqual(double d1, double d2) {
            return System.Math.Abs(d1 - d2) <= EPSILON;
        }
    }
    public class Vector2 {
        private double x;
        private double y;

        private double magnitude;
        private bool magnitudeHasChanged;

        private double direction;
        private bool directionHasChanged;

        public double X {
            get => this.x;
            set {
                this.magnitudeHasChanged = true;
                this.directionHasChanged = true;

                this.x = value;
            }
        }

        public double Y {
            get => this.y;
            set {
                this.magnitudeHasChanged = true;
                this.directionHasChanged = true;

                this.y = value;
            }
        }

        public double Magnitude {
            get {
                if(this.magnitudeHasChanged) {
                    this.magnitude = System.Math.Sqrt(this.MagnitudeSquared);
                    this.magnitudeHasChanged = false;
                }

                return this.magnitude;
            }

            set {
                Vector2 newCoords = this.Normal * value;
                this.x = newCoords.X;
                this.y = newCoords.Y;
                this.magnitude = value;
            }
        }

        private double Direction {
            get {
                if(this.directionHasChanged) {
                    this.direction = System.Math.Atan2(this.Y, this.X);
                    this.directionHasChanged = false;
                }

                return this.direction;
            }

            set {
                this.x = this.Magnitude * System.Math.Sin(value);
                this.y = this.Magnitude * System.Math.Cos(value);
                this.direction = value;
            }
        }

        public double MagnitudeSquared {
            get => this.Dot(this);
        }

        public Vector2 Normal {
            get => this / this.Magnitude;
        }

        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public double Dot(Vector2 v) =>
            this.X * v.X + this.Y * v.Y;

        public static Vector2 operator +(Vector2 v1, Vector2 v2) =>
            new Vector2(v1.X + v2.X, v1.Y + v2.Y);

        public static Vector2 operator -(Vector2 v) =>
            new Vector2(-v.X, -v.Y);

        public static Vector2 operator -(Vector2 v1, Vector2 v2) =>
            v1 + (-v2);

        public static Vector2 operator *(double s1, Vector2 v2) =>
            new Vector2(s1 * v2.X, s1 * v2.Y);

        public static Vector2 operator *(Vector2 v1, double s2) =>
            new Vector2(s2 * v1.X, s2 * v1.Y);

        public static Vector2 operator /(Vector2 v1, double s2) =>
            v1 * (1.0 / s2);
    }

    public class Line2 {
        private Vector2 start;
        private Vector2 end;

        private double length;

        private bool isDirty;

        public Line2(Vector2 start, Vector2 end) {
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