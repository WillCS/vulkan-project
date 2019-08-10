using System;

namespace Project.Math {

    public delegate double PointEmbedding(Vector2 pos2d);

    public class SurfaceTopology {
        
    }

    public class SurfaceEmbedding {

        private PointEmbedding x;
        private PointEmbedding y;
        private PointEmbedding z;

        public SurfaceEmbedding(PointEmbedding x, PointEmbedding y, PointEmbedding z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double X(Vector2 pos2d) => this.x(pos2d);

        public double X(double x, double y) => this.x(new Vector2(x, y));

        public double Y(Vector2 pos2d) => this.y(pos2d);

        public double Y(double x, double y) => this.y(new Vector2(x, y));

        public double Z(Vector2 pos2d) => this.z(pos2d);

        public double Z(double x, double y) => this.z(new Vector2(x, y));

        public Vector3 Embed(Vector2 pos2d) 
                => new Vector3(this.x(pos2d), this.y(pos2d), this.z(pos2d));

        public Vector3 Embed(double x, double y) => this.Embed(new Vector2(x, y));
    }
}