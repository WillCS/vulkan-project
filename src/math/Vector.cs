namespace Game.Math {
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

        #endregion Properties

        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        #region Methods

        public double Dot(Vector2 v) =>
            this.X * v.X + this.Y * v.Y;

        public override bool Equals(object obj) {
            if (obj == null || obj is Vector2) {
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

        #endregion OperatorOverloads
    }

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
            if (obj == null || obj is Vector3) {
                return false;
            }
            Vector3 v = obj as Vector3;
            return
                MathHelper.ApproximatelyEqual(this.X, v.X) &&
                MathHelper.ApproximatelyEqual(this.Y, v.Y) &&
                MathHelper.ApproximatelyEqual(this.Z, v.Z);
        }
        
        public override int GetHashCode() {
            // redo this :(
            return double.Parse($"{this.X}.{this.Y}").GetHashCode();
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

        #endregion OperatorOverloads
    }
}
