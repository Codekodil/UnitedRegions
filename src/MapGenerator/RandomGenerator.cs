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

            var startNode = graph.NewNode();
            TransformNode(startNode);
            var endNode = graph.NewNode();
            endNode.Center.Y = 75;
            TransformNode(endNode);
            var thirdNode = graph.NewNode();
            thirdNode.Center.Y = 200;
            TransformNode(thirdNode);
            var forthNode = graph.NewNode();
            forthNode.Center.Y = 140;
            forthNode.Center.X = 70;
            TransformNode(forthNode);

            var route1 = graph.NewEdge(startNode, endNode, "Route 1", Seed);
            var route2 = graph.NewEdge(endNode, thirdNode, "Route 2", Seed);
            var route3 = graph.NewEdge(endNode, forthNode, "Route 3", Seed);
            route3.Center = (route3.Center + new Coord(forthNode.Center.X, endNode.Center.Y) * 2) / 3;

            return graph;
        }
        private void TransformNode(MapNode node)
        {
            node.Center += new Coord(_randomGraph.Next(-10, 11), _randomGraph.Next(-10, 11));
        }



    }
}
