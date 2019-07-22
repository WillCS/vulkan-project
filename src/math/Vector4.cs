namespace Game.Math {

    public class Vector4 {

        #region StaticFields

        public static Vector4 IDENTITY {
            get => new Vector4(1, 1, 1, 1);
        }

        public static Vector4 ZERO {
            get => new Vector4(0, 0, 0, 0);
        }
        
        public static Vector4 UNIT_X {
            get => new Vector4(1, 0, 0, 0);
        }
        
        public static Vector4 UNIT_Y {
            get => new Vector4(0, 1, 0, 0);
        }
        
        public static Vector4 UNIT_Z {
            get => new Vector4(0, 0, 1, 0);
        }
        
        public static Vector4 UNIT_W {
            get => new Vector4(0, 0, 0, 1);
        }

        #endregion StaticFields

        #region PrivateFields

        private double x;
        private double y;
        private double z;
        private double w;

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

        public double W {
            get => this.w;
            set {
                this.magnitudeHasChanged = true;

                this.w = value;
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
                Vector4 newCoords = this.Normal * value;
                this.x = newCoords.X;
                this.y = newCoords.Y;
                this.z = newCoords.Z;
                this.w = newCoords.W;
                this.magnitude = value;
            }
        }

        public double MagnitudeSquared {
            get => this.Dot(this);
        }

        public Vector4 Normal {
            get => this / this.Magnitude;
        }

        #endregion Properties

        public Vector4(double x, double y, double z, double w) {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        #region Methods

        public double Dot(Vector4 v) =>
            this.X * v.X + this.Y * v.Y + this.Z * v.Z + this.W * v.W;

        public override bool Equals(object obj) {
            if (obj == null || obj is Vector4) {
                return false;
            }
            Vector4 v = obj as Vector4;
            return
                MathHelper.ApproximatelyEqual(this.X, v.X) &&
                MathHelper.ApproximatelyEqual(this.Y, v.Y) &&
                MathHelper.ApproximatelyEqual(this.Z, v.Z) &&
                MathHelper.ApproximatelyEqual(this.W, v.W);
        }
        
        public override int GetHashCode() {
            // redo this :(
            return double.Parse($"{this.X}.{this.Y}").GetHashCode();
        }

        public override string ToString() {
            return $"({this.X}, {this.Y}, {this.Z}, {this.W})";
        }

        #endregion Methods

        #region OperatorOverloads

        public static Vector4 operator +(Vector4 v1, Vector4 v2) =>
            new Vector4(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v1.W);

        public static Vector4 operator -(Vector4 v) =>
            new Vector4(-v.X, -v.Y, -v.Z, -v.W);

        public static Vector4 operator -(Vector4 v1, Vector4 v2) =>
            v1 + (-v2);

        public static Vector4 operator *(double s1, Vector4 v2) =>
            new Vector4(s1 * v2.X, s1 * v2.Y, s1 * v2.Z, s1 * v2.W);

        public static Vector4 operator *(Vector4 v1, double s2) =>
            new Vector4(s2 * v1.X, s2 * v1.Y, s2 * v1.Z, s2 * v1.W);

        public static Vector4 operator /(Vector4 v1, double s2) =>
            v1 * (1.0 / s2);

        #endregion OperatorOverloads
    }
}
