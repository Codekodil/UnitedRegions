using MapData;
using System.Collections.Generic;
using System.Linq;

namespace MapGenerator
{
    internal class MapNode
    {
        public Coord Center;
        public int Height;

        public readonly List<Connection> Connections = new List<Connection>();
        public class Connection
        {
            public MapEdge Edge;
            public Coord Dock, Direction;
            public int Height;
        }

        public Connection GetConnection(MapEdge edge) => Connections.First(c => c.Edge == edge);
    }
}
