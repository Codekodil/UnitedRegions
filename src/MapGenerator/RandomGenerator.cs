using MapData;
using System;
using System.Linq;

namespace MapGenerator
{
    public partial class RandomGenerator
    {
        internal static readonly ETile RoutePlaceholder;
        internal static readonly ETile ForestPlaceholder;
        static RandomGenerator()
        {
            RoutePlaceholder = Enum.GetValues(typeof(ETile)).Cast<ETile>().Max() + 1;
            ForestPlaceholder = RoutePlaceholder + 1;
        }


        public int Seed { get; }
        private readonly Random _randomGraph;
        private readonly Random _randomNodes;
        public RandomGenerator(int seed)
        {
            Seed = seed;
            var random = new Random(Seed);
            _randomGraph = new Random(random.Next() / 3);
            _randomNodes = new Random(random.Next() / 3);
        }

        public Map Map { get; private set; }

        public void GenerateMap()
        {
            if (Map != null) return;
            Map = new Map(123);

            var graph = GenerateGraph();

            foreach (var node in graph.Nodes)
                AddNodeToMap(node);
            foreach (var edge in graph.Edges)
                EdgeRandomGenerator.AddEdgeToMap(edge, Map);

            //foreach (var chunk in map.GetChunks())
            //    for (var x = 0; x < MapChunk.ChunkSize; ++x)
            //        for (var y = 0; y < MapChunk.ChunkSize; ++y)
            //            if (chunk.GetTile(x, y) == ETile.Undefined)
            //                chunk.SetTile(x, y, ForestPlaceholder);

            //foreach (var chunk in map.GetChunks())
            //{
            //    for (var x = 0; x < MapChunk.ChunkSize; ++x)
            //        for (var y = 0; y < MapChunk.ChunkSize; ++y)
            //        {
            //            var tile = chunk.GetTile(x, y);
            //            if (tile == ForestPlaceholder)
            //            {
            //                if (x % 2 == 0 && y % 2 == 0)
            //                {
            //                    chunk.SetTile(x, y, ETile.Tree);
            //                    chunk.SetGround(x, y, EGround.Grass);
            //                }
            //                else
            //                {
            //                    chunk.SetTile(x, y, ETile.Blocked);
            //                    chunk.SetGround(x, y, EGround.Grass);
            //                }
            //            }
            //        }
            //}

            MapImprover.SmoothGroundEdges(Map);
            MapImprover.SmoothSmallCliffs(Map);
            MapImprover.DefineUndefined(Map);
        }





        private MapGraph GenerateGraph()
        {
            var graph = new MapGraph();

            MapNode N(int x, int y)
            {
                var n = graph.NewNode();
                n.Center.X = x;
                n.Center.Y = y;
                TransformNode(n);
                return n;
            }

            var a = N(0, 0);
            var b = N(0, 75);
            var c = N(0, 200);
            var d = N(70, 140);
            var e = N(140, 40);
            var f = N(-50, 60);

            var route1 = graph.NewEdge(a, b, "Route 1", Seed);
            var route2 = graph.NewEdge(b, c, "Route 2", Seed);
            var route3 = graph.NewEdge(b, d, "Route 3", Seed); route3.Center = (route3.Center + new Coord(d.Center.X, b.Center.Y) * 2) / 3;
            var route4 = graph.NewEdge(c, d, "Route 4", Seed); route4.Center = (route4.Center + new Coord(d.Center.X, c.Center.Y) * 2) / 3;
            var route5 = graph.NewEdge(d, e, "Route 5", Seed); route5.Center = (route5.Center + new Coord(e.Center.X, d.Center.Y) * 2) / 3;
            var route6 = graph.NewEdge(a, e, "Route 6", Seed);
            var route7 = graph.NewEdge(a, f, "Route 7", Seed); route7.Center = (route7.Center + new Coord(f.Center.X, a.Center.Y) * 2) / 3;

            return graph;
        }
        private void TransformNode(MapNode node)
        {
            node.Center += new Coord(_randomGraph.Next(-10, 11), _randomGraph.Next(-10, 11));
        }



    }
}
