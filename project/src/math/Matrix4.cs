using System;
using System.Linq;
using System.Text;
using Project.Native;

namespace Project.Math {

    public class Matrix4 {

        #region StaticFields

        public static Matrix4 IDENTITY {
            get => new Matrix4(Vector4.UNIT_X, Vector4.UNIT_Y, Vector4.UNIT_Z, Vector4.UNIT_W);
        }

        public static Matrix4 ZERO {
            get => new Matrix4(Vector4.ZERO, Vector4.ZERO, Vector4.ZERO, Vector4.ZERO);
        }

        #endregion StaticFields

        #region PrivateFields

        // Row Major
        private double[] elements;

        #endregion PrivateFields

        #region Properties

        public double Determinant {
            get => 
                this[0, 0] * this.GetMinor(0, 0).Determinant -
                this[0, 1] * this.GetMinor(0, 1).Determinant +
                this[0, 2] * this.GetMinor(0, 2).Determinant -
                this[0, 3] * this.GetMinor(0, 3).Determinant;
        }

        public Matrix4 Transpose {
            get => new Matrix4(
                this.GetRow(0),
                this.GetRow(1),
                this.GetRow(2),
                this.GetRow(3));
        }

        public Matrix4 Cofactor {
            get {
                Matrix4 newMatrix = Matrix4.ZERO;

                for(int row = 0; row < 4; row++) {
                    for(int column = 0; column < 4; column++) {
                        newMatrix[row, column] = ((row + column) % 2 == 0 ? 1 : -1) * this[row, column];
                    }
                }

                return newMatrix;
            }
        }

        public Matrix4 Inverse {
            get {
                Matrix4 minors = Matrix4.ZERO;

                for(int row = 0; row < 4; row++) {
                    for(int column = 0; column < 4; column++) {
                        minors[row, column] = this.GetMinor(row, column).Determinant;
                    }
                }

                return minors.Cofactor.Transpose / this.Determinant;
            }
        }

        #endregion Properties

        public Matrix4(double[] elements) {
            if(elements.Length != 16) {
                throw new Exception("WRONG SIZE");
            }

            this.elements = elements;
        }

        public Matrix4(Vector4 col1, Vector4 col2, Vector4 col3, Vector4 col4) {
            this.elements = new double[] {
                col1.X, col2.X, col3.X, col4.X,
                col1.Y, col2.Y, col3.Y, col4.Y,
                col1.Z, col2.Z, col3.Z, col4.Z,
                col1.W, col2.W, col3.W, col4.W
            };
        }

        #region Methods

        public Matrix3 GetMinor(int row, int column) {
            double[] minorElements = new double[9];
            int minorIndex = 0;

            for(int r = 0; r < 4; r++) {
                for(int c = 0; c < 4; c++) {
                    if(r == row || c == column) {
                        continue;
                    } else {
                        minorElements[minorIndex++] = this[r, c];
                    }
                }
            }

            return new Matrix3(minorElements);
        }

        public double this[int row, int column] {
            get => this.elements[row * 4 + column];
            set => this.elements[row * 4 + column] = value;
        }

        public Vector4 GetRow(int row) {
            if(row == 0 || row == 1 || row == 2 || row == 3) {
                return new Vector4(this[row, 0], this[row, 1], this[row, 2], this[row, 3]);
            } else {
                throw new IndexOutOfRangeException($"3x3 matrix does not have a row {row}.");
            }
        }

        public Vector4 GetColumn(int column) {
            if(column == 0 || column == 1 || column == 2 || column == 3) {
                return new Vector4(this[0, column], this[1, column], this[2, column], this[3, column]);
            } else {
                throw new IndexOutOfRangeException($"3x3 matrix does not have a column {column}.");
            }
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is Matrix4)) {
                return false;
            }

            Matrix4 m = obj as Matrix4;
            return m.elements.Select<double, bool>((double element, int index) => 
                MathHelper.ApproximatelyEqual(element, this.elements[index])
            ).All((bool isTrue) => isTrue);

        }

        public override int GetHashCode() {
            return this.elements.GetHashCode();
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();

            for(int row = 0; row < 4; row++) {
                builder.Append("[ ");
                for(int col = 0; col < 4; col++) {
                    builder.Append($"{this[row, col]} ");
                }
                builder.Append("]");

                if(row != 3) {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }

        #endregion Methods

        #region StaticFunctions

        #region OperatorOverloads

        public static Matrix4 operator +(Matrix4 m1, Matrix4 m2) {
            double[] elements = new double[16];

            for(int i = 0; i < 16; i++) {
                elements[i] = m1.elements[i] + m2.elements[i];
            }

            return new Matrix4(elements);
        }

        public static Matrix4 operator -(Matrix4 m1, Matrix4 m2) =>
            m1 + -m2;

        public static Matrix4 operator -(Matrix4 m) =>
            -1 * m;

        public static Matrix4 operator *(double s1, Matrix4 m2) {
            double[] elements = new double[16];

            for(int i = 0; i < 16; i++) {
                elements[i] = s1 * m2.elements[i];
            }

            return new Matrix4(elements);
        }

        public static Matrix4 operator *(Matrix4 m1, double s2) =>
            s2 * m1;

        public static Matrix4 operator *(Matrix4 m1, Matrix4 m2) {
            Matrix4 newMatrix = Matrix4.ZERO;

            for(int row = 0; row < 4; row++) {
                for(int column = 0; column < 4; column++) {
                    newMatrix[row, column] = m1.GetRow(row).Dot(m2.GetColumn(column));
                }
            }

            return newMatrix;
        }

        public static Matrix4 operator /(Matrix4 m1, double s2) =>
            m1 * (1 / s2);

        public static Vector4 operator *(Matrix4 m1, Vector4 v2) =>
            new Vector4(
                m1.GetRow(0).Dot(v2), 
                m1.GetRow(1).Dot(v2), 
                m1.GetRow(2).Dot(v2),
                m1.GetRow(3).Dot(v2));

        public static implicit operator Mat4(Matrix4 managed) {
            Mat4 unmanaged = new Mat4();

            for(int row = 0; row < 4; row++) {
                for(int column = 0; column < 4; column++) {
                    unmanaged.elems[row * 4 + column] = (float) managed[row, column];
                }
            }

            return unmanaged;
        }

        #endregion OperatorOverloads

        #endregion StaticFunctions
    }
}
