using System;
using System.Runtime.InteropServices;
using Game.Native;

namespace Game.Math {

    public class Vector3 {

        #region StaticFields

        public static Vector3 IDENTITY {
            get => new Vector3(1, 1, 1);
        }

        public static Vector3 ZERO {
            get => new Vector3(0, 0, 0);
        }
        
        public static Vector3 UNIT_X {
            get => new Vector3(1, 0, 0);
        }
        
        public static Vector3 UNIT_Y {
            get => new Vector3(0, 1, 0);
        }
        
        public static Vector3 UNIT_Z {
            get => new Vector3(0, 0, 1);
        }

        #endregion StaticFields

        #region PrivateFields

        private double x;
        private double y;
        private double z;

        private double magnitude;
        private bool magnitudeHasChanged;

        #endregion PrivateFields

        #region Properties

        public double X {
            get => this.x;
            set {
                this.magnitudeHasChanged = true;

                this.x = value;
            }
        }

        public double Y {
            get => this.y;
            set {
                this.magnitudeHasChanged = true;

                this.y = value;
            }
        }

        public double Z {
            get => this.z;
            set {
                this.magnitudeHasChanged = true;

                this.z = value;
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
                Vector3 newCoords = this.Normal * value;
                this.x = newCoords.X;
                this.y = newCoords.Y;
                this.z = newCoords.Z;
                this.magnitude = value;
            }
        }

        public double MagnitudeSquared {
            get => this.Dot(this);
        }

        public Vector3 Normal {
            get => this / this.Magnitude;
        }

        #endregion Properties

        public Vector3(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        #region Methods

        public double Dot(Vector3 v) =>
            this.X * v.X + this.Y * v.Y + this.Z * v.Z;

        public Vector3 Cross(Vector3 v) =>
            new Vector3(
                this.Y * v.Z - this.Z * v.Y,
                this.Z * v.X - this.X * v.Z,
                this.X * v.Y - this.Y * v.X);

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Vector3)) {
                return false;
            }

            Vector3 v = (Vector3) obj;
            return
                MathHelper.ApproximatelyEqual(this.X, v.X) &&
                MathHelper.ApproximatelyEqual(this.Y, v.Y) &&
                MathHelper.ApproximatelyEqual(this.Z, v.Z);
        }
        
        public override int GetHashCode() {
            return (new double[] {this.X, this.Y, this.Z}).GetHashCode();
        }

        public override string ToString() {
            return $"({this.X}, {this.Y}, {this.Z})";
        }

        #endregion Methods

        #region OperatorOverloads

        public static Vector3 operator +(Vector3 v1, Vector3 v2) =>
            new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public static Vector3 operator -(Vector3 v) =>
            new Vector3(-v.X, -v.Y, -v.Z);

        public static Vector3 operator -(Vector3 v1, Vector3 v2) =>
            v1 + (-v2);

        public static Vector3 operator *(double s1, Vector3 v2) =>
            new Vector3(s1 * v2.X, s1 * v2.Y, s1 * v2.Z);

        public static Vector3 operator *(Vector3 v1, double s2) =>
            new Vector3(s2 * v1.X, s2 * v1.Y, s2 * v1.Z);

        public static Vector3 operator /(Vector3 v1, double s2) =>
            v1 * (1.0 / s2);

        public static implicit operator Vec3(Vector3 managed) {
            var native = new Vec3();
            native.X = (float) managed.X;
            native.Y = (float) managed.Y;
            native.Z = (float) managed.Z;
            return native;
        }

        public static implicit operator DVec3(Vector3 managed) {
            var native = new DVec3();
            native.X = managed.X;
            native.Y = managed.Y;
            native.Z = managed.Z;
            return native;
        }

        #endregion OperatorOverloads
    }
}
