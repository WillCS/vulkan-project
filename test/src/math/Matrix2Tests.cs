using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Matrix2Tests {
        private Matrix2 matrix;

        [SetUp]
        public void Setup() {
            this.matrix = new Matrix2(2, 5, 4, 8);
        }

        [Test]
        public void Matrix2Equality() {
            var same      = new Matrix2(2, 5, 4, 8);
            var different = new Matrix2(5, 2, 8, 4);

            Assert.AreEqual(this.matrix, same);
            Assert.AreNotEqual(different, this.matrix);
        }

        [Test]
        public void Matrix2Indexing() {
            Assert.AreEqual(2, this.matrix[0, 0], MathHelper.EPSILON);
            Assert.AreEqual(5, this.matrix[0, 1], MathHelper.EPSILON);
            Assert.AreEqual(4, this.matrix[1, 0], MathHelper.EPSILON);
            Assert.AreEqual(8, this.matrix[1, 1], MathHelper.EPSILON);
        }

        public void Matrix2Components() {
            var row0 = new Vector2(2, 5);
            var row1 = new Vector2(4, 8);
            var col0 = new Vector2(2, 4);
            var col1 = new Vector2(5, 8);

            Assert.AreEqual(row0, this.matrix.GetRow(0));
            Assert.AreEqual(row1, this.matrix.GetRow(1));
            Assert.AreEqual(col0, this.matrix.GetColumn(0));
            Assert.AreEqual(col1, this.matrix.GetColumn(1));
        }

        [Test]
        public void Matrix2ScalarMultiplication() {
            var scalar  = 2;
            var product = new Matrix2(4, 10, 8, 16);

            Assert.AreEqual(product, this.matrix * scalar);
        }

        [Test]
        public void Matrix2ScalarDivision() {
            var scalar   = 2;
            var quotient = new Matrix2(1, 2.5, 2, 4);

            Assert.AreEqual(quotient, this.matrix / scalar);
        }

        [Test]
        public void Matrix2Addition() {
            var addend = new Matrix2(1, 2, 3, 4);
            var sum    = new Matrix2(3, 7, 7, 12);

            Assert.AreEqual(sum, this.matrix + addend);
        }

        [Test]
        public void Matrix2Subtraction() {
            var minuend    = new Matrix2(1, 2, 3, 4);
            var difference = new Matrix2(1, 3, 1, 4);

            Assert.AreEqual(difference, this.matrix - minuend);
        }

        [Test]
        public void Matrix2Multiplication() {
            var multiplicand = new Matrix2(1, 2, 3, 4);
            var product      = new Matrix2(17, 24, 28, 40);

            Assert.AreEqual(product, this.matrix * multiplicand);
            Assert.AreEqual(this.matrix, this.matrix * Matrix2.IDENTITY);
        }

        public void Matrix2Transpose() {
            var transpose = new Matrix2(2, 4, 5, 8);

            Assert.AreEqual(transpose, this.matrix.Transpose);
        }

        [Test]
        public void Matrix2Determinant() {
            var determinant = 2 * 8 - 5 * 4;

            Assert.AreEqual(determinant, this.matrix.Determinant);
        }

        [Test]
        public void Matrix2Inverse() {
            var inverse = new Matrix2(-2, 5.0/4, 1, -1.0/2);

            Assert.AreEqual(inverse, this.matrix.Inverse);
            Assert.AreEqual(Matrix2.IDENTITY, this.matrix * inverse);
        }
    }
}