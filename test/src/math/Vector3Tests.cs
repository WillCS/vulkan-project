using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Vector3Tests {
        private Vector3 vector;

        [SetUp]
        public void Setup() {
            this.vector = new Vector3(2, 5, 4);
        }

        [Test]
        public void TestEquality() {
            var same      = new Vector3(2, 5, 4);
            var different = new Vector3(5, 4, 2);

            Assert.AreEqual(this.vector, same);
            Assert.AreNotEqual(different, this.vector);
        }

        [Test]
        public void TestScalarMultiplication() {
            var scalar  = 2;
            var product = new Vector3(4, 10, 8);

            Assert.AreEqual(product, this.vector * scalar);
        }

        [Test]
        public void TestScalarDivision() {
            var scalar   = 2;
            var quotient = new Vector3(1, 2.5, 2);

            Assert.AreEqual(quotient, this.vector / scalar);
        }

        [Test]
        public void TestVectorAddition() {
            var addend = new Vector3(1, 2, 3);
            var sum    = new Vector3(3, 7, 7);

            Assert.AreEqual(sum, this.vector + addend);
        }

        [Test]
        public void TestVectorSubtraction() {
            var minuend    = new Vector3(1, 2, 3);
            var difference = new Vector3(1, 3, 1);

            Assert.AreEqual(difference, this.vector - minuend);
        }

        [Test]
        public void TestDotProduct() {
            var other   = new Vector3(1, 2, 3);
            var product = 2 * 1 + 5 * 2 + 4 * 3;

            Assert.AreEqual(product, this.vector.Dot(other));
        }

        [Test]
        public void TestCrossProduct() {
            var other   = new Vector3(1, 2, 3);
            var product = new Vector3(
                      5 * 3 - 4 * 2,
                    -(2 * 3 - 4 * 1),
                      2 * 2 - 5 * 1);

            Assert.AreEqual(product, this.vector.Cross(other));
            Assert.AreEqual(-product, other.Cross(this.vector));
        }

        [Test]
        public void TestMatrixMultiplication() {
            var matrix  = new Matrix3(new double[] {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            });
            var product = new Vector3(24, 57, 90);

            Assert.AreEqual(product, matrix * this.vector);
        }

        [Test]
        public void TestNormal() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5 + 4 * 4);
            var normal = new Vector3(2 / magnitude, 5 / magnitude, 4 / magnitude);

            Assert.AreEqual(normal, this.vector.Normal);
        }

        [Test]
        public void TestMagnitudeValue() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5 + 4 * 4);

            Assert.AreEqual(magnitude, this.vector.Magnitude, MathHelper.EPSILON);
        }

        [Test]
        public void TestChangeMagnitude() {
            var vectorCopy = new Vector3(2, 5, 4);
            var scaled     = new Vector3(4, 10, 8);
            
            vectorCopy.Magnitude *= 2;

            Assert.AreEqual(scaled, vectorCopy);
            Assert.AreEqual(this.vector.Normal, vectorCopy.Normal);
        }
    }
}