using NUnit.Framework;
using Project.Math;

namespace Tests.Math {
    public class Vector2Tests {
        private Vector2 vector;

        [SetUp]
        public void Setup() {
            this.vector = new Vector2(2, 5);
        }

        [Test]
        public void TestEquality() {
            var same      = new Vector2(2, 5);
            var different = new Vector2(5, 2);

            Assert.AreEqual(this.vector, same);
            Assert.AreNotEqual(different, this.vector);
        }

        [Test]
        public void TestScalarMultiplication() {
            var scalar  = 2;
            var product = new Vector2(4, 10);

            Assert.AreEqual(product, this.vector * scalar);
        }

        [Test]
        public void TestScalarDivision() {
            var scalar   = 2;
            var quotient = new Vector2(1, 2.5);

            Assert.AreEqual(quotient, this.vector / scalar);
        }

        [Test]
        public void TestVectorAddition() {
            var addend = new Vector2(1, 2);
            var sum    = new Vector2(3, 7);

            Assert.AreEqual(sum, this.vector + addend);
        }

        [Test]
        public void TestVectorSubtraction() {
            var minuend    = new Vector2(1, 2);
            var difference = new Vector2(1, 3);

            Assert.AreEqual(difference, this.vector - minuend);
        }

        [Test]
        public void TestDotProduct() {
            var other   = new Vector2(1, 2);
            var product = 2 * 1 + 5 * 2;

            Assert.AreEqual(product, this.vector.Dot(other));
        }

        [Test]
        public void TestMatrixMultiplication() {
            var matrix  = new Matrix2(1, 2, 3, 4);
            var product = new Vector2(12, 26);

            Assert.AreEqual(product, matrix * this.vector);
        }

        [Test]
        public void TestNormal() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5);
            var normal = new Vector2(2 / magnitude, 5 / magnitude);

            Assert.AreEqual(normal, this.vector.Normal);
        }

        [Test]
        public void TestGradient() {
            var gradient = 5 / 2.0;

            Assert.AreEqual(gradient, this.vector.Gradient, MathHelper.EPSILON);
        }

        [Test]
        public void TestMagnitudeValue() {
            var magnitude = System.Math.Sqrt(2 * 2 + 5 * 5);

            Assert.AreEqual(magnitude, this.vector.Magnitude, MathHelper.EPSILON);
        }

        [Test]
        public void TestDirectionValue() {
            var direction = System.Math.Atan2(5, 2);

            Assert.AreEqual(direction, this.vector.Direction, MathHelper.EPSILON);
        }

        [Test]
        public void TestChangeMagnitude() {
            var vectorCopy = new Vector2(2, 5);
            var scaled     = new Vector2(4, 10);
            
            vectorCopy.Magnitude *= 2;

            Assert.AreEqual(scaled, vectorCopy);
            Assert.AreEqual(this.vector.Direction, vectorCopy.Direction, MathHelper.EPSILON);
            Assert.AreEqual(this.vector.Normal, vectorCopy.Normal);
        }

        [Test]
        public void TestChangeDirection() {
            var vectorCopy = new Vector2( 2,  5);
            var rotated    = new Vector2(-5, -2);

            vectorCopy.Direction += System.Math.PI;

            Assert.AreEqual(rotated, vectorCopy);
            Assert.AreEqual(this.vector.Magnitude, vectorCopy.Magnitude);
        }
    }
}