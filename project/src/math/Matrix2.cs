using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Native;

namespace Project.Math {
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
            get => new Matrix2(this.d, -this.b, -this.c, this.a) / this.Determinant;
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
            if (obj == null || !(obj is Matrix2)) {
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

        public override string ToString()
            => $"[ {this.a} {this.b} ] [ {this.c} {this.d} ]";

        #endregion Methods

        #region StaticFunctions

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
                m1.c * m2.a + m1.d * m2.c,
                m1.c * m2.b + m1.d * m2.d);

        public static Matrix2 operator /(Matrix2 m1, double s2) =>
            m1 * (1 / s2);

        public static Vector2 operator *(Matrix2 m1, Vector2 v2) =>
            new Vector2(m1.a * v2.X + m1.b * v2.Y, m1.c * v2.X + m1.d * v2.Y);

        public static implicit operator Mat2(Matrix2 managed) {
            Mat2 unmanaged = new Mat2();
            unmanaged.A = (float) managed.a;
            unmanaged.B = (float) managed.b;
            unmanaged.C = (float) managed.c;
            unmanaged.D = (float) managed.d;

            return unmanaged;
        }

        #endregion OperatorOverloads

        #endregion StaticFunctions
    }
}
