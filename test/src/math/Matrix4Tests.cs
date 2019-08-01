using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Matrix4Tests {
        private Matrix4 matrix;

        [SetUp]
        public void Setup() {
            this.matrix = new Matrix4(new double[] {
                2, 5, 7, 5,
                4, 8, 3, 2,
                9, 6, 1, 9,
                4, 8, 3, 7
            });
        }

        [Test]
        public void Matrix4Equality() {
            var same = new Matrix4(new double[] {
                2, 5, 7, 5,
                4, 8, 3, 2,
                9, 6, 1, 9,
                4, 8, 3, 7
            });

            Assert.AreEqual(this.matrix, same);
        }

        [Test]
        public void Matrix4Indexing() {
            var elems = new double[] {
                2, 5, 7, 5,
                4, 8, 3, 2,
                9, 6, 1, 9,
                4, 8, 3, 7
            };

            for(int row = 0; row < 4; row++) {
                for(int col = 0; col < 4; col++) {
                    Assert.AreEqual(elems[row * 4 + col], this.matrix[row, col]);
                }
            }
        }

        public void Matrix4Components() {
            var row0 = new Vector4(2, 5, 7, 5);
            var row1 = new Vector4(4, 8, 3, 2);
            var row2 = new Vector4(9, 6, 1, 9);
            var row3 = new Vector4(4, 8, 3, 7);
            var col0 = new Vector4(2, 4, 9, 4);
            var col1 = new Vector4(5, 8, 6, 8);
            var col2 = new Vector4(7, 3, 1, 3);
            var col3 = new Vector4(5, 2, 9, 7);

            Assert.AreEqual(row0, this.matrix.GetRow(0));
            Assert.AreEqual(row1, this.matrix.GetRow(1));
            Assert.AreEqual(row2, this.matrix.GetRow(2));
            Assert.AreEqual(col0, this.matrix.GetColumn(0));
            Assert.AreEqual(col1, this.matrix.GetColumn(1));
            Assert.AreEqual(col2, this.matrix.GetColumn(2));
        }

        [Test]
        public void Matrix4ScalarMultiplication() {
            var scalar  = 2;
            var product = new Matrix4(new double[] {
                 4, 10, 14, 10,
                 8, 16,  6,  4,
                18, 12,  2, 18,
                 8, 16,  6, 14
            });

            Assert.AreEqual(product, this.matrix * scalar);
        }

        [Test]
        public void Matrix4ScalarDivision() {
            var scalar   = 2;
            var quotient = new Matrix4(new double[] {
                  1, 2.5, 3.5, 2.5,
                  2,   4, 1.5,   1,
                4.5,   3, 0.5, 4.5,
                  2,   4, 1.5, 3.5
            });

            Assert.AreEqual(quotient, this.matrix / scalar);
        }

        [Test]
        public void Matrix4Addition() {
            var addend = new Matrix4(new double[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 1, 2,
                3, 4, 5, 6
            });

            var sum    = new Matrix4(new double[] {
                 3,  7, 10,  9,
                 9, 14, 10, 10,
                18,  6,  2, 11,
                 7, 12,  8, 13
            });

            Assert.AreEqual(sum, this.matrix + addend);
        }

        [Test]
        public void Matrix4Subtraction() {
            var minuend    = new Matrix4(new double[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 1, 2,
                3, 4, 5, 6
            });

            var difference = new Matrix4(new double[] {
                 1, 3,  4,  1,
                -1, 2, -4, -6,
                 0, 6,  0,  7,
                 1, 4, -2,  1
            });

            Assert.AreEqual(difference, this.matrix - minuend);
        }

        [Test]
        public void Matrix4Multiplication() {
            var multiplicand    = new Matrix4(new double[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 1, 2,
                3, 4, 5, 6
            });

            var product         = new Matrix4(new double[] {
                105, 54,  73,  92,
                 77, 64,  81,  98,
                 75, 90, 115, 140,
                 92, 84, 106, 128
            });

            Assert.AreEqual(product, this.matrix * multiplicand);
            Assert.AreEqual(this.matrix, this.matrix * Matrix4.IDENTITY);
        }

        public void Matrix4Transpose() {
            var transpose = new Matrix4(new double[] {
                2, 4, 9, 4,
                5, 8, 6, 8,
                7, 3, 1, 3,
                5, 2, 9, 7
            });

            Assert.AreEqual(transpose, this.matrix.Transpose);
        }

        [Test]
        public void Matrix4Determinant() {
            var determinant = -1205;

            Assert.AreEqual(determinant, this.matrix.Determinant);
        }

        [Test]
        public void Matrix4Inverse() {
            var inverse = new Matrix4(new double[] {
                 10,    32,  41,  -69,
                -23,  22.8, -22, 38.2,
                 48,     9,   4,  -42,
                  0, -48.2,   0, 48.2 
            }) / 241;

            Assert.AreEqual(inverse, this.matrix.Inverse);
            Assert.AreEqual(Matrix4.IDENTITY, this.matrix * inverse);
        }
    }
}