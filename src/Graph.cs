using System.Collections.Generic;

namespace Game.Math.Graph {
    public interface IGraph<T> where T : IVertex<T> {
        IEnumerable<T> Vertices {
            get;
        }

        IEnumerable<T> NeighboursOf(T vertex);
    }

    public interface IVertex<T> where T : IVertex<T> {
        IEnumerable<T> Neighbours {
            get;
        }
    }

    /* A general purpose graph implemented using an adjacency list.
     * We use an adjacency list because pretty much every kind of graph
     * we need is going to be sparse. If I need a dense graph at some
     * point I'll implement an adjacency matrix version too. */
    public class Graph : IGraph<Vertex> {
        private Dictionary<Vertex, List<Vertex>> adjacencyList;

        public IEnumerable<Vertex> Vertices {
            get {
                return this.adjacencyList.Keys;
            }
        }

        public IEnumerable<Vertex> NeighboursOf(Vertex vertex) {
            return this.adjacencyList[vertex];
        }
    }

    public class Vertex: IVertex<Vertex> {
        private Graph graph;

        public IEnumerable<Vertex> Neighbours {
            get {
                return this.graph.NeighboursOf(this);
            }
        }
    }
}