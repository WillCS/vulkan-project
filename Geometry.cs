using System;

namespace Game.Geometry {
    public class Vector2 {
        public double X;
        public double Y;

        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public Vector2 Dot(Vector2 v) =>
            new Vector2(this.X * v.X, this.Y * v.Y);

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
                    this.length = Math.Sqrt(this.LengthSquared);
                    this.isDirty = false;
                }

                return this.length;
            }
        }
    }
}