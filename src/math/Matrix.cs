using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game.Math {
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

        public Matrix2(Vector2 ac, Vector2 bd) {
            this.a = ac.X;
            this.c = ac.Y;
            this.b = bd.X;
            this.d = bd.Y;
        }

        #region Methods

        public double this[int row, int column] {
            get {
                switch(row) {
                    case 0 when column == 0: 
                        return this.a;
                    case 0 when column == 1:
                        return this.b;
                    case 1 when column == 0:
                        return this.c;
                    case 1 when column == 1:
                        return this.d;
                    default:
                        throw new IndexOutOfRangeException($"({row}, {column}) not in 2x2 matrix");
                }
            }

            set {
                switch(row) {
                    case 0 when column == 0: 
                        this.a = value;
                        break;
                    case 0 when column == 1:
                        this.b = value;
                        break;
                    case 1 when column == 0:
                        this.c = value;
                        break;
                    case 1 when column == 1:
                        this.d = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"({row}, {column}) not in 2x2 matrix");
                }
            }
        }

        public Vector2 GetRow(int row) {
            switch(row) {
                case 0: return new Vector2(this.a, this.d);
                case 1: return new Vector2(this.b, this.d);
                default:
                    throw new IndexOutOfRangeException($"2x2 matrix does not have a row {row}.");
            }
        }

        public Vector2 GetColumn(int column) {
            switch(column) {
                case 0: return new Vector2(this.a, this.b);
                case 1: return new Vector2(this.c, this.d);
                default:
                    throw new IndexOutOfRangeException($"2x2 matrix does not have a column {column}.");
            }
        }

        public override bool Equals(object obj) {
            if (obj == null || obj is Matrix2) {
                return false;
            }

            Matrix2 m = obj as Matrix2;
            return 
                MathHelper.ApproximatelyEqual(this.a, m.a) && 
                MathHelper.ApproximatelyEqual(this.b, m.b) && 
                MathHelper.ApproximatelyEqual(this.c, m.c) && 
                MathHelper.ApproximatelyEqual(this.d, m.d);

        }

        public override int GetHashCode() {
            int code1 = new Vector2(this.a, this.b).GetHashCode();
            int code2 = new Vector2(this.c, this.d).GetHashCode();

            return new Vector2(code1, code2).GetHashCode();
        }

        #endregion Methods

        #region StaticFunctions

        public static Matrix2 RotationMatrix(double angle) {
            double sin = System.Math.Sin(angle);
            double cos = System.Math.Cos(angle);
            return new Matrix2(cos, -sin, sin, cos);
        }

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

    public class Matrix3 {

        #region StaticFields

        public static Matrix3 IDENTITY {
            get => new Matrix3(Vector3.UNIT_X, Vector3.UNIT_Y, Vector3.UNIT_Z);
        }

        public static Matrix3 ZERO {
            get => new Matrix3(Vector3.ZERO, Vector3.ZERO, Vector3.ZERO);
        }

        #endregion StaticFields

        #region PrivateFields

        // Column Major
        private double[] elements;

        #endregion PrivateFields

        #region Properties

        public double Determinant {
            get => 
                this[0, 0] * this.GetMinor(0, 0).Determinant -
                this[0, 1] * this.GetMinor(0, 1).Determinant +
                this[0, 2] * this.GetMinor(0, 2).Determinant;
        }

        public Matrix3 Transpose {
            get => new Matrix3(
                this.GetRow(0),
                this.GetRow(1),
                this.GetRow(2));
        }

        public Matrix3 Cofactor {
            get {
                Matrix3 newMatrix = Matrix3.ZERO;

                for(int row = 0; row < 3; row++) {
                    for(int column = 0; column < 3; column++) {
                        newMatrix[row, column] = ((row + column) % 2 == 0 ? 1 : -1) * this[row, column];
                    }
                }

                return newMatrix;
            }
        }

        public Matrix3 Inverse {
            get {
                Matrix3 minors = Matrix3.ZERO;

                for(int row = 0; row < 3; row++) {
                    for(int column = 0; column < 3; column++) {
                        minors[row, column] = this.GetMinor(row, column).Determinant;
                    }
                }

                return minors.Cofactor.Transpose / this.Determinant;
            }
        }

        #endregion Properties

        public Matrix3(double[] elements) {
            if(elements.Length != 9) {
                throw new Exception("WRONG SIZE");
            }

            this.elements = elements;
        }

        public Matrix3(Vector3 col1, Vector3 col2, Vector3 col3) {
            this.elements = new double[] {
                col1.X, col2.X, col3.X,
                col1.Y, col2.Y, col3.Y,
                col1.Z, col2.Z, col3.Z
            };
        }

        #region Methods

        public Matrix2 GetMinor(int row, int column) {
            double[] minorElements = new double[4];
            int minorIndex = 0;

            for(int r = 0; r < 3; r++) {
                for(int c = 0; c < 3; c++) {
                    if(r == row || c == column) {
                        continue;
                    } else {
                        minorElements[minorIndex++] = this[r, c];
                    }
                }
            }

            return new Matrix2(
                minorElements[0], minorElements[1],
                minorElements[2], minorElements[3]);
        }

        public double this[int row, int column] {
            get => this.elements[row + 3 * column];
            set => this.elements[row + 3 * column] = value;
        }

        public Vector3 GetRow(int row) {
            if(row == 0 || row == 1 || row == 2) {
                int elementIndex = row;
                return new Vector3(elementIndex, elementIndex + 3, elementIndex + 6);
            } else {
                throw new IndexOutOfRangeException($"2x2 matrix does not have a row {row}.");
            }
        }

        public Vector3 GetColumn(int column) {
            if(column == 0 || column == 1 || column == 2) {
                int elementIndex = column * 3;
                return new Vector3(elementIndex, elementIndex + 1, elementIndex + 2);
            } else {
                throw new IndexOutOfRangeException($"2x2 matrix does not have a column {column}.");
            }
        }

        public override bool Equals(object obj) {
            if (obj == null || obj is Matrix3) {
                return false;
            }

            Matrix3 m = obj as Matrix3;
            return m.elements.Select<double, bool>((double element, int index) => 
                MathHelper.ApproximatelyEqual(element, this.elements[index])
            ).Count() == 9;

        }

        public override int GetHashCode() {
            return this.elements.GetHashCode();
        }

        #endregion Methods

        #region StaticFunctions

        #region OperatorOverloads

        public static Matrix3 operator +(Matrix3 m1, Matrix3 m2) {
            double[] elements = new double[9];

            for(int i = 0; i < 9; i++) {
                elements[i] = m1.elements[i] + m1.elements[i];
            }

            return new Matrix3(elements);
        }

        public static Matrix3 operator -(Matrix3 m1, Matrix3 m2) =>
            m1 + -m2;

        public static Matrix3 operator -(Matrix3 m) =>
            -1 * m;

        public static Matrix3 operator *(double s1, Matrix3 m2) {
            double[] elements = new double[9];

            for(int i = 0; i < 9; i++) {
                elements[i] = s1 * m2.elements[i];
            }

            return new Matrix3(elements);
        }

        public static Matrix3 operator *(Matrix3 m1, double s2) =>
            s2 * m1;

        public static Matrix3 operator *(Matrix3 m1, Matrix3 m2) {
            Matrix3 newMatrix = Matrix3.ZERO;

            for(int row = 0; row < 3; row++) {
                for(int column = 0; column < 3; column++) {
                    newMatrix[row, column] = m1.GetRow(row).Dot(m2.GetColumn(column));
                }
            }

            return newMatrix;
        }

        public static Matrix3 operator /(Matrix3 m1, double s2) =>
            m1 * (1 / s2);

        public static Vector3 operator *(Matrix3 m1, Vector3 v2) =>
            new Vector3(
                m1.GetRow(0).Dot(v2), 
                m1.GetRow(1).Dot(v2), 
                m1.GetRow(2).Dot(v2));

        #endregion OperatorOverloads

        #endregion StaticFunctions
    }
}
