namespace Game.Math {
    public static class MathHelper {
        public static readonly double EPSILON = 0.001D;

        public static bool ApproximatelyEqual(double d1, double d2) {
            return System.Math.Abs(d1 - d2) <= EPSILON;
        }

        public static double Square(double num) {
            return num * num;
        }

        public static double IntegerPower(double num, int power) {
            if(power < 0) {
                return 1 / IntegerPower(num, -power);
            } else if(power == 0) {
                return 1;
            } else if(power == 1) {
                return num;
            } else if(power == 2) {
                return Square(num);
            } else {
                return num * IntegerPower(num, power - 1);
            }
        }

        public static uint Clamp(uint lowerBound, uint between, uint upperBound) {
            return System.Math.Max(lowerBound, System.Math.Min(between, upperBound));
        }
    }
}
