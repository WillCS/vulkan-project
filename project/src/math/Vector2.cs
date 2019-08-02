using System.Runtime.InteropServices;
using Project.Native;

namespace Project.Math {
    public class Vector2 {

        #region StaticFields

        public static Vector2 IDENTITY {
            get => new Vector2(1, 1);
        }

        public static Vector2 ZERO {
            get => new Vector2(0, 0);
        }

        public static Vector2 UNIT_X {
            get => new Vector2(1, 0);
        }

        public static Vector2 UNIT_Y {
            get => new Vector2(0, 1);
        }

        #endregion StaticFields

        #region PrivateFields

        private double x;
        private double y;

        private double magnitude;
        private bool magnitudeHasChanged;

        private double direction;
        private bool directionHasChanged;

        #endregion PrivateFields

        #region Properties

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

        public double Gradient {
            get => this.Y / this.X;
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

        public double Direction {
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

        #endregion Properties

        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        #region Methods

        public double Dot(Vector2 v) =>
            this.X * v.X + this.Y * v.Y;

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Vector2)) {
                return false;
            }
            Vector2 v = obj as Vector2;
            return 
                MathHelper.ApproximatelyEqual(this.X, v.X) &&
                MathHelper.ApproximatelyEqual(this.Y, v.Y);
        }
        
        public override int GetHashCode() {
            // redo this :(
            return double.Parse($"{this.X}.{this.Y}").GetHashCode();
        }

        public override string ToString() {
            return $"({this.X}, {this.Y})";
        }

        #endregion Methods

        #region OperatorOverloads

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

        public static implicit operator Vec2(Vector2 managed) {
            var native = new Vec2();
            native.X = (float) managed.X;
            native.Y = (float) managed.Y;
            return native;
        }

        public static implicit operator DVec2(Vector2 managed) {
            var native = new DVec2();
            native.X = managed.X;
            native.Y = managed.Y;
            return native;
        }

        #endregion OperatorOverloads
    }
}
