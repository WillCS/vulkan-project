namespace Project.Math {
    public static class Matrices {
        public static Matrix2 RotationMatrix2(double angle) {
            var cos = System.Math.Cos(angle);
            var sin = System.Math.Sin(angle);
            
            return new Matrix2(
                cos, -sin,
                sin,  cos);
        }

        public static Matrix3 FromMatrix2(Matrix2 m) => new Matrix3(new double[] {
            m[0, 0], m[0, 1], 0,
            m[1, 0], m[1, 1], 0,
                  0,       0, 1
        });

        public static Matrix3 XRotationMatrix3(double angle) {
            var cos = System.Math.Cos(angle);
            var sin = System.Math.Sin(angle);

            return new Matrix3(new double[] {
                1,   0,    0,
                0, cos, -sin,
                0, sin,  cos
            });
        }

        public static Matrix3 YRotationMatrix3(double angle) {
            var cos = System.Math.Cos(angle);
            var sin = System.Math.Sin(angle);

            return new Matrix3(new double[] {
                 cos, 0, sin,
                   0, 1,   0,
                -sin, 0, cos
            });
        }

        public static Matrix3 ZRotationMatrix3(double angle) {
            var cos = System.Math.Cos(angle);
            var sin = System.Math.Sin(angle);

            return new Matrix3(new double[] {
                cos, -sin, 0,
                sin,  cos, 0,
                  0,    0, 1
            });
        }

        public static Matrix3 RotationMatrix3(double x, double y, double z) {
            var rotX = XRotationMatrix3(x);
            var rotY = YRotationMatrix3(y);
            var rotZ = ZRotationMatrix3(z);

            return rotZ * rotY * rotX;
        }

        public static Matrix4 FromMatrix3(Matrix3 m) => new Matrix4(new double[] {
            m[0, 0], m[0, 1], m[0, 2], 0,
            m[1, 0], m[1, 1], m[1, 2], 0,
            m[2, 0], m[2, 1], m[2, 2], 0,
                  0,       0,       0, 1
        });

        public static Matrix4 XRotationMatrix4(double angle) => 
                FromMatrix3(XRotationMatrix3(angle));

        public static Matrix4 YRotationMatrix4(double angle) => 
                FromMatrix3(YRotationMatrix3(angle));

        public static Matrix4 ZRotationMatrix4(double angle) => 
                FromMatrix3(ZRotationMatrix3(angle));

        public static Matrix4 RotationMatrix4(double x, double y, double z) =>
                FromMatrix3(RotationMatrix3(x, y, z));

        public static Matrix4 PerspectiveProjectionMatrix(double near, double far, 
                double aspect, double fov) {
            double fovScaleFactor = 1/(System.Math.Tan(fov / 2.0));
            double left = -1;
            double right = 1;
            double top = -1;
            double bottom = 1;

            Matrix4 projection = Matrix4.ZERO;
            projection[0, 0] = aspect * fovScaleFactor * (right - left);
            projection[1, 1] = fovScaleFactor * (bottom - top);
            projection[2, 2] = -far / (far - near);
            projection[3, 2] = -(far * near) / (far - near);
            projection[2, 3] = -1;

            return projection;
        }
    }
}
