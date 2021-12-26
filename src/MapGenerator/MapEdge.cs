using MapData;
using System;

namespace MapGenerator
{
    internal class MapEdge
    {
        public Coord Center;
        public MapNode From, To;
        public string Name;
        public Random Rand;
    }
}
