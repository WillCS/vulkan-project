using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Matrix3Tests {
        private Matrix3 matrix;

        [SetUp]
        public void Setup() {
            this.matrix = new Matrix3(new double[] {
                2, 5, 7,
                4, 8, 3,
                9, 6, 1
            });
        }

        [Test]
        public void Matrix3Equality() {
            var same = new Matrix3(new double[] {
                2, 5, 7,
                4, 8, 3,
                9, 6, 1
            });

            var different = new Matrix3(new double[] {
                2, 4, 9,
                5, 8, 6,
                7, 3, 1
            });

            Assert.AreEqual(this.matrix, same);
            Assert.AreNotEqual(different, this.matrix);
        }

        [Test]
        public void Matrix3Indexing() {
            var elems = new double[] {
                2, 5, 7,
                4, 8, 3,
                9, 6, 1
            };

            for(int row = 0; row < 3; row++) {
                for(int col = 0; col < 3; col++) {
                    Assert.AreEqual(elems[row * 3 + col], this.matrix[row, col]);
                }
            }
        }

        public void Matrix3Components() {
            var row0 = new Vector3(2, 5, 7);
            var row1 = new Vector3(4, 8, 3);
            var row2 = new Vector3(9, 6, 1);
            var col0 = new Vector3(2, 4, 9);
            var col1 = new Vector3(5, 8, 6);
            var col2 = new Vector3(7, 3, 1);

            Assert.AreEqual(row0, this.matrix.GetRow(0));
            Assert.AreEqual(row1, this.matrix.GetRow(1));
            Assert.AreEqual(row2, this.matrix.GetRow(2));
            Assert.AreEqual(col0, this.matrix.GetColumn(0));
            Assert.AreEqual(col1, this.matrix.GetColumn(1));
            Assert.AreEqual(col2, this.matrix.GetColumn(2));
        }

        [Test]
        public void Matrix3ScalarMultiplication() {
            var scalar  = 2;
            var product = new Matrix3(new double[] {
                 4, 10, 14,
                 8, 16,  6,
                18, 12,  2
            });

            Assert.AreEqual(product, this.matrix * scalar);
        }

        [Test]
        public void Matrix3ScalarDivision() {
            var scalar   = 2;
            var quotient = new Matrix3(new double[] {
                1, 2.5, 3.5,
                2, 4, 1.5,
                4.5, 3, 0.5
            });

            Assert.AreEqual(quotient, this.matrix / scalar);
        }

        [Test]
        public void Matrix3Addition() {
            var addend = new Matrix3(new double[] {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            });

            var sum    = new Matrix3(new double[] {
                 3,  7, 10,
                 8, 13,  9,
                16, 14, 10
            });

            Assert.AreEqual(sum, this.matrix + addend);
        }

        [Test]
        public void Matrix3Subtraction() {
            var minuend    = new Matrix3(new double[] {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            });

            var difference = new Matrix3(new double[] {
                 1,  3,  4,
                 0,  3, -3,
                 2, -2, -8
            });

            Assert.AreEqual(difference, this.matrix - minuend);
        }

        [Test]
        public void Matrix3Multiplication() {
            var multiplicand    = new Matrix3(new double[] {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            });

            var product         = new Matrix3(new double[] {
                71, 85, 99,
                57, 72, 87,
                40, 56, 72
            });

            Assert.AreEqual(product, this.matrix * multiplicand);
            Assert.AreEqual(this.matrix, this.matrix * Matrix3.IDENTITY);
        }

        public void Matrix3Transpose() {
            var transpose = new Matrix3(new double[] {
                2, 4, 9,
                5, 8, 6,
                7, 3, 1
            });

            Assert.AreEqual(transpose, this.matrix.Transpose);
        }

        [Test]
        public void Matrix3Determinant() {
            var determinant = -241;

            Assert.AreEqual(determinant, this.matrix.Determinant);
        }

        [Test]
        public void Matrix3Inverse() {
            var inverse = new Matrix3(new double[] {
                 10, -37,  41,
                -23,  61, -22,
                 48, -33,   4 
            }) / 241;

            Assert.AreEqual(inverse, this.matrix.Inverse);
            Assert.AreEqual(Matrix3.IDENTITY, this.matrix * inverse);
        }
    }
}