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

    public class Matrix : IEnumerable<double> {
        private int rows;
        private int columns;

        /// <summary>
        ///     Elements of the matrix are stored in row major order
        /// </summary>
        private double[] elements;

        public Matrix(int rows, int columns) {
            this.rows = rows;
            this.columns = columns;
            this.elements = new double[rows * columns];
        }

        public Matrix(int rows, int columns, IEnumerable<double> elements) {
            if(rows * columns != elements.Count()) {
                throw new Exception();
            }

            this.rows = rows;
            this.columns = columns;
            this.elements = elements.ToArray();
        }

        public int Rows {
            get => this.rows;
        }

        public int Columns {
            get => this.columns;
        }

        public bool IsSquare {
            get => this.rows == this.columns;
        }

        public double this[int row, int column] {
            get {
                if(row >= this.rows || column >= this.columns) {
                    throw new System.IndexOutOfRangeException();
                }

                return this.elements[this.columns * row + column];
            }

            set {
                if(row >= this.rows || column >= this.columns) {
                    throw new System.IndexOutOfRangeException();
                }

                this.elements[this.columns * row + column] = value;
            }
        }

        public Matrix GetRow(int row) {
            if(row >= this.rows) {
                throw new System.IndexOutOfRangeException();
            }

            Matrix rowVector = new Matrix(1, this.columns);
            for(int i = 0; i < this.columns; i++) {
                rowVector[0, i] = this[row, i];
            }

            return rowVector;
        }

        public Matrix GetColumn(int column) {
            if(column >= this.columns) {
                throw new System.IndexOutOfRangeException();
            }

            Matrix columnVector = new Matrix(this.rows, 1);
            for(int i = 0; i < this.rows; i++) {
                columnVector[i, 0] = this[i, column];
            }

            return columnVector;
        }

        public static Matrix operator +(Matrix m1, Matrix m2) {
            if(m1.Rows != m2.Rows || m1.Columns != m2.Columns) {
                throw new Exception();
            }

            return new Matrix(m1.Rows, m1.Columns,
                    m1.Zip(m2, (double e1, double e2) => e1 + e2).ToArray());
        }

        public static Matrix operator -(Matrix m) =>
            new Matrix(m.Rows, m.Columns,
                    m.Select((double e, int i) => -e).ToArray());

        public static Matrix operator -(Matrix m1, Matrix m2) =>
            m1 + (-m2);

        public static Matrix operator *(double s, Matrix m) =>
            new Matrix(m.Rows, m.Columns, 
                    m.Select((double e, int i) => s * e).ToArray());

        public static Matrix operator *(Matrix m, double s) =>
            s * m;

        public static Matrix operator /(Matrix m, double s) =>
            m * (1.0 / s);

        public static Matrix operator *(Matrix m1, Matrix m2) {
            if(m1.Columns != m2.Rows) {
                throw new Exception();
            }

            Matrix newMatrix = new Matrix(m1.Rows, m2.Columns);

            for(int i = 0; i < m1.Rows; i++) {
                for(int j = 0; j < m2.Columns; j++) {
                    newMatrix[i, j] = m1.GetRow(i).Zip(
                        m2.GetColumn(j), (e1, e2) => e1 * e1).Sum();
                }
            }

            return newMatrix;
        }

        public IEnumerator<double> GetEnumerator() => 
            this.elements.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this.GetEnumerator();
    }
}
