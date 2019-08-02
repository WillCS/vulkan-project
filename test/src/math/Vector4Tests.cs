using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Vector4Tests {
        private Vector4 vector;

        [SetUp]
        public void Setup() {
            this.vector = new Vector4(2, 5, 4, 7);
        }

        [Test]
        public void TestEquality() {
            var same      = new Vector4(2, 5, 4, 7);
            var different = new Vector4(5, 4, 7, 2);

            Assert.AreEqual(this.vector, same);
            Assert.AreNotEqual(different, this.vector);
        }

        [Test]
        public void TestScalarMultiplication() {
            var scalar  = 2;
            var product = new Vector4(4, 10, 8, 14);

            Assert.AreEqual(product, this.vector * scalar);
        }

        [Test]
        public void TestScalarDivision() {
            var scalar   = 2;
            var quotient = new Vector4(1, 2.5, 2, 3.5);

            Assert.AreEqual(quotient, this.vector / scalar);
        }

        [Test]
        public void TestVectorAddition() {
            var addend = new Vector4(1, 2, 3, 4);
            var sum    = new Vector4(3, 7, 7, 11);

            Assert.AreEqual(sum, this.vector + addend);
        }

        [Test]
        public void TestVectorSubtraction() {
            var minuend    = new Vector4(1, 2, 3, 4);
            var difference = new Vector4(1, 3, 1, 3);

            Assert.AreEqual(difference, this.vector - minuend);
        }

        [Test]
        public void TestDotProduct() {
            var other   = new Vector4(1, 2, 3, 4);
            var product = 2 * 1 + 5 * 2 + 4 * 3 + 7 * 4;

            Assert.AreEqual(product, this.vector.Dot(other));
        }

        [Test]
        public void TestMatrixMultiplication() {
            var matrix  = new Matrix4(new double[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 1, 2,
                3, 4, 5, 6
            });
            var product = new Vector4(52, 124, 36, 88);

            Assert.AreEqual(product, matrix * this.vector);
        }

        [Test]
        public void TestNormal() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5 + 4 * 4 + 7 * 7);
            var normal = new Vector4(2 / magnitude, 5 / magnitude, 4 / magnitude, 7 / magnitude);

            Assert.AreEqual(normal, this.vector.Normal);
        }

        [Test]
        public void TestMagnitudeValue() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5 + 4 * 4 + 7 * 7);

            Assert.AreEqual(magnitude, this.vector.Magnitude, MathHelper.EPSILON);
        }

        [Test]
        public void TestChangeMagnitude() {
            var vectorCopy = new Vector4(2, 5, 4, 7);
            var scaled     = new Vector4(4, 10, 8, 14);
            
            vectorCopy.Magnitude *= 2;

            Assert.AreEqual(scaled, vectorCopy);
            Assert.AreEqual(this.vector.Normal, vectorCopy.Normal);
        }
    }
}