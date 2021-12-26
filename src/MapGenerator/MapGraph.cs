using System;
using System.Collections.Generic;
using System.Linq;

namespace MapGenerator
{
    internal class MapGraph
    {

        private readonly List<MapNode> _nodes = new List<MapNode>();
        private readonly List<MapEdge> _edges = new List<MapEdge>();

        public IReadOnlyList<MapNode> Nodes => _nodes.AsReadOnly();
        public IReadOnlyList<MapEdge> Edges => _edges.AsReadOnly();

        public MapNode NewNode()
        {
            var node = new MapNode();
            _nodes.Add(node);
            return node;
        }

        public MapEdge NewEdge(MapNode from, MapNode to, string name, int seed)
        {
            var edge = new MapEdge
            {
                From = from,
                To = to,
                Name = name,
                Rand = new Random(Tuple.Create($"Edge{name}", seed).GetHashCode())
            };
            edge.Center = (from.Center + to.Center) / 2;
            from.Connections.Add(new MapNode.Connection { Edge = edge });
            to.Connections.Add(new MapNode.Connection { Edge = edge });
            _edges.Add(edge);
            return edge;
        }

        public IEnumerable<MapEdge> ConnectedEdges(MapNode node) =>
            _edges.Where(e => e.From == node || e.To == node);

        public IEnumerable<MapNode> ConnectedNodes(MapNode node) =>
            ConnectedEdges(node).Select(e => e.From == node ? e.To : e.From);
    }
}
