using System;

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

        public static Matrix4 TranslationMatrix4(double x, double y, double z) =>
                new Matrix4(
                        Vector4.UNIT_X, 
                        Vector4.UNIT_Y, 
                        Vector4.UNIT_Z, 
                        new Vector4(x, y, z, 1));

        public static Matrix4 TranslationMatrix4(Vector3 translation) =>
                TranslationMatrix4(translation.X, translation.Y, translation.Z);

        public static Matrix4 PerspectiveProjectionMatrix(double near, double far, 
                double aspect, double fov, bool correctForVulkan = true) {
            double f = System.Math.Tan(fov / 2.0);
            double range = 1 / (near - far);

            double a = f / aspect;
            double b = f;
            double c = (near + far) * range;
            double d = near * far * range * 2;
            
            var matrix = new Matrix4(new double[] {
                a, 0,  0, 0,
                0, b,  0, 0,
                0, 0,  c, d,
                0, 0, -1, 0
            });

            if(correctForVulkan) {
                return VulkanProjectionCorrectionMatrix() * matrix;
            } else {
                return matrix;
            }
        }

        public static Matrix4 OrthographicProjectionMatrix(double near, double far, 
                double width, double height, bool correctForVulkan = true) {
            double a = (2 * near) / width;
            double b = (2 * near) / height;
            double c = -2 / (far - near);
            double d = -(far + near) / (far - near);

            var matrix = new Matrix4(new double[] {
                a, 0, 0, 0,
                0, b, 0, 0,
                0, 0, c, d,
                0, 0, 0, 1
            });

            if(correctForVulkan) {
                return VulkanProjectionCorrectionMatrix() * matrix;
            } else {
                return matrix;
            }
        }

        public static Matrix4 IsometricTransformationMatrix(int quadrant = 0, bool below = false) {
            var quad  = quadrant % 4;
            var alpha = System.Math.Asin(System.Math.Tan(System.Math.PI / 6));
            var beta  = System.Math.PI / 4;
            var sign  = below ? -1 : 1;

            var xRotationMatrix  = XRotationMatrix4(alpha);
            var yRotationMatrix  = YRotationMatrix4(beta);

            return xRotationMatrix * yRotationMatrix;
        }

        public static Matrix4 IsometricLookMatrix(Vector3 target, double distance, int quadrant = 0, bool below = false) {
            var translationMatrix    = TranslationMatrix4(-target);
            var transformationMatrix = IsometricTransformationMatrix(quadrant, below);

            var viewMatrix           = TranslationMatrix4(0, -distance, 0);

            return transformationMatrix * translationMatrix * viewMatrix;
        }

        public static Matrix4 VulkanProjectionCorrectionMatrix() {
            return new Matrix4(new double[] {
                1,  0,   0,   0,
                0, -1,   0,   0,
                0,  0, 0.5, 0.5,
                0,  0,   0,   1
            });
        }

        public static Matrix4 LookAtMatrix(Vector3 camera, Vector3 target, Vector3 up) {
            var zRotation = (camera - target).Normal;
            var xRotation = (up.Cross(zRotation)).Normal;
            var yRotation = zRotation.Cross(xRotation);

            var rotationMatrix = FromMatrix3(new Matrix3(xRotation, yRotation, zRotation).Transpose);
            var translationMatrix = TranslationMatrix4(-camera);

            return rotationMatrix * translationMatrix;
        }

        public static Matrix4 LookAtMatrix(Vector3 camera, Vector3 target) =>
                LookAtMatrix(camera, target, Vector3.UNIT_Y);
    }
}
