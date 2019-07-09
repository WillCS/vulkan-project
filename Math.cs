namespace Game.Math {

    public class MathHelper {
        public static readonly double EPSILON = 0.001D;
        public static bool ApproximatelyEqual(double d1, double d2) {
            return System.Math.Abs(d1 - d2) <= EPSILON;
        }

        public static double Square(double num) {
            return num * num;
        }

        public static double IntegerPower(double num, int power) {
            if(power < 0) {
                return 1 / IntegerPower(num, -power);
            } else if(power == 0) {
                return 1;
            } else if(power == 1) {
                return num;
            } else if(power == 2) {
                return Square(num);
            } else {
                return num * IntegerPower(num, power - 1);
            }
        }
    }
    
    public class Vector2 {
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
            return this.X == v.X && this.Y == v.Y;
        }
        
        public override int GetHashCode() {
            // redo this :(
            return double.Parse($"{this.X}.{this.Y}").GetHashCode();
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

    public class Matrix2 {

        #region StaticFields

        public static Matrix2 IDENTITY {
            get => new Matrix2(1, 0, 0, 1);
        }

        public static Matrix2 ZERO {
            get => new Matrix2(0, 0, 0, 0);
        }

        #endregion StaticFields

        #region PrivateFields

        // [a b]
        // [c d]

        private double a;
        private double b;
        private double c;
        private double d;

        #endregion PrivateFields

        #region Properties

        public double Determinant {
            get => this.a * this.d - this.b * this.c;
        }

        public Matrix2 Transpose {
            get => new Matrix2(this.a, this.c, this.b, this.d);
        }

        public Matrix2 Inverse {
            get => this.Determinant * new Matrix2(this.d, -this.b, -this.c, this.a);
        }

        #endregion Properties

        public Matrix2(double a, double b, double c, double d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        #region Methods

        public override bool Equals(object obj) {
            if (obj == null || obj is Matrix2) {
                return false;
            }

            Matrix2 m = obj as Matrix2;
            return this.a == m.a && this.b == m.b && this.c == m.c && this.d == m.d;

        }

        public override int GetHashCode() {
            int code1 = new Vector2(this.a, this.b).GetHashCode();
            int code2 = new Vector2(this.c, this.d).GetHashCode();

            return new Vector2(code1, code2).GetHashCode();
        }

        #region StaticFunctions

        public static Matrix2 RotationMatrix(double angle) =>
            new Matrix2(
                System.Math.Cos(angle), -System.Math.Sin(angle),
                System.Math.Sin(angle), System.Math.Cos(angle));

        #region OperatorOverloads

        public static Matrix2 operator +(Matrix2 m1, Matrix2 m2) =>
            new Matrix2(m1.a + m2.a, m1.b + m2.b, m1.c + m2.c, m1.d + m2.d);

        public static Matrix2 operator -(Matrix2 m1, Matrix2 m2) =>
            m1 + -m2;

        public static Matrix2 operator -(Matrix2 m) =>
            -1 * m;

        public static Matrix2 operator *(double s1, Matrix2 m2) =>
            new Matrix2(s1 * m2.a, s1 * m2.b, s1 * m2.c, s1 * m2.d);

        public static Matrix2 operator *(Matrix2 m1, double s2) =>
            s2 * m1;

        public static Matrix2 operator *(Matrix2 m1, Matrix2 m2) =>
            new Matrix2(
                m1.a * m2.a + m1.b * m2.c, 
                m1.a * m2.b + m1.b * m2.d,
                m1.b * m2.a + m1.d * m2.c,
                m1.b * m2.b + m1.d * m2.d);

        public static Matrix2 operator /(Matrix2 m1, double s2) =>
            m1 * (1 / s2);

        public static Vector2 operator *(Matrix2 m1, Vector2 v2) =>
            new Vector2(m1.a * v2.X + m1.b * v2.Y, m1.c * v2.X + m1.d * v2.Y);

        #endregion OperatorOverloads

        #endregion StaticFunctions
    }
}