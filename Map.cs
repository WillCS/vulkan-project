using System.Collections.Generic;
using Game.Math;

namespace Game {
    public class Floor {

        private int xSize;
        private int ySize;
        private int[,] floorTiles;

        private Dictionary<int, string> tileMappings;

        protected Floor() {
            
        }

        public int XSize { 
            get => this.xSize;
        }

        public int YSize {
            get => this.ySize;
        }

        public bool CoordIsInRange(int x, int y) =>
            0 <= x && x <= this.XSize && 0 <= y && y <= this.YSize;

        public string TileAt(int x, int y) {
            if(!this.CoordIsInRange(x, y)) {
                throw new System.Exception("Coordinate out of range.");
            }

            int tileID = floorTiles[x, y];
            
            if(!this.tileMappings.ContainsKey(tileID)) {
                throw new System.Exception("Tile ID not found.");
            }

            return this.tileMappings[tileID];
        }
    }
}
