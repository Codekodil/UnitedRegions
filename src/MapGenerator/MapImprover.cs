using MapData;
using MapData.Tile;
using System;
using System.Linq;

namespace MapGenerator
{
    public static class MapImprover
    {
        public static void SmoothGroundEdges(Map map)
        {
            foreach (var chunk in map.GetChunks())
                GenerateEdges(chunk);
        }
        public static void GenerateEdges(MapChunk chunk)
        {
            var groundCached = new EGround[MapChunk.ChunkSize + 2, MapChunk.ChunkSize + 2];

            for (var x = -1; x <= MapChunk.ChunkSize; ++x)
                for (var y = -1; y <= MapChunk.ChunkSize; ++y)
                    groundCached[x + 1, y + 1] = chunk.GetGroundExtended(x, y);

            for (var x = 0; x < MapChunk.ChunkSize; ++x)
                for (var y = 0; y < MapChunk.ChunkSize; ++y)
                {
                    var edge = EEdge.None;
                    try
                    {
                        var ground = groundCached[x + 1, y + 1];
                        if (ground == EGround.Grass) continue;

                        var edges = 0;
                        if (groundCached[x + 1, y + 2] == ground) edges |= 1;
                        if (groundCached[x + 2, y + 1] == ground) edges |= 2;
                        if (groundCached[x + 1, y] == ground) edges |= 4;
                        if (groundCached[x, y + 1] == ground) edges |= 8;

                        switch (edges)
                        {
                            case 15:
                                edges = 0;
                                if (groundCached[x + 2, y + 2] == ground) edges |= 1;
                                if (groundCached[x + 2, y] == ground) edges |= 2;
                                if (groundCached[x, y] == ground) edges |= 4;
                                if (groundCached[x, y + 2] == ground) edges |= 8;
                                switch (edges)
                                {
                                    case 14: edge = EEdge.InnerCornerTopRight; break;
                                    case 13: edge = EEdge.InnerCornerBottomRight; break;
                                    case 11: edge = EEdge.InnerCornerBottomLeft; break;
                                    case 7: edge = EEdge.InnerCornerTopLeft; break;
                                }
                                break;
                            case 14: edge = EEdge.Top; break;
                            case 13: edge = EEdge.Right; break;
                            case 11: edge = EEdge.Bottom; break;
                            case 7: edge = EEdge.Left; break;

                            case 12: edge = EEdge.CornerTopRight; break;
                            case 9: edge = EEdge.CornerBottomRight; break;
                            case 3: edge = EEdge.CornerBottomLeft; break;
                            case 6: edge = EEdge.CornerTopLeft; break;
                        }
                    }
                    finally
                    {
                        chunk.SetEdge(x, y, edge);
                    }
                }
        }


        public static void SmoothSmallCliffs(Map map)
        {
            foreach (var chunk in map.GetChunks())
                SmoothSmallCliffs(chunk);
        }
        public static void SmoothSmallCliffs(MapChunk chunk)
        {
            var tileCached = new ETile[MapChunk.ChunkSize + 2, MapChunk.ChunkSize + 2];
            var edgeCached = new EEdge[MapChunk.ChunkSize + 2, MapChunk.ChunkSize + 2];


            for (var x = -1; x <= MapChunk.ChunkSize; ++x)
                for (var y = -1; y <= MapChunk.ChunkSize; ++y)
                {
                    tileCached[x + 1, y + 1] = chunk.GetTileExtended(x, y);
                    edgeCached[x + 1, y + 1] = chunk.GetDataExtended(x, y) is EEdge e ? e : default;
                }

            //Corners
            {
                //TODO
            }

            //Ends
            {
                for (var x = 0; x < MapChunk.ChunkSize; ++x)
                    for (var y = 0; y < MapChunk.ChunkSize; ++y)
                        if (tileCached[x + 1, y + 1] == ETile.SmallCliffs)
                        {
                            EEdge ConnectionTop()
                            {
                                if (tileCached[x + 1, y + 2] != ETile.SmallCliffs) return EEdge.None;
                                switch (edgeCached[x + 1, y + 2])
                                {
                                    case EEdge.Right:
                                    case EEdge.CornerTopRight:
                                    case EEdge.InnerCornerBottomRight:
                                    case EEdge.RightCutTop:
                                        return EEdge.Right;
                                    case EEdge.Left:
                                    case EEdge.CornerTopLeft:
                                    case EEdge.InnerCornerBottomLeft:
                                    case EEdge.LeftCutTop:
                                        return EEdge.Left;
                                }
                                return EEdge.None;
                            }
                            EEdge ConnectionRight()
                            {
                                if (tileCached[x + 2, y + 1] != ETile.SmallCliffs) return EEdge.None;
                                switch (edgeCached[x + 2, y + 1])
                                {
                                    case EEdge.Top:
                                    case EEdge.CornerTopRight:
                                    case EEdge.InnerCornerTopLeft:
                                    case EEdge.TopCutRight:
                                        return EEdge.Top;
                                    case EEdge.Bottom:
                                    case EEdge.CornerBottomRight:
                                    case EEdge.InnerCornerBottomLeft:
                                    case EEdge.BottomCutRight:
                                        return EEdge.Bottom;
                                }
                                return EEdge.None;
                            }
                            EEdge ConnectionBottom()
                            {
                                if (tileCached[x + 1, y] != ETile.SmallCliffs) return EEdge.None;
                                switch (edgeCached[x + 1, y])
                                {
                                    case EEdge.Right:
                                    case EEdge.CornerBottomRight:
                                    case EEdge.InnerCornerTopRight:
                                    case EEdge.RightCutBottom:
                                        return EEdge.Right;
                                    case EEdge.Left:
                                    case EEdge.CornerBottomLeft:
                                    case EEdge.InnerCornerTopLeft:
                                    case EEdge.LeftCutBottom:
                                        return EEdge.Left;
                                }
                                return EEdge.None;
                            }
                            EEdge ConnectionLeft()
                            {
                                if (tileCached[x, y + 1] != ETile.SmallCliffs) return EEdge.None;
                                switch (edgeCached[x, y + 1])
                                {
                                    case EEdge.Top:
                                    case EEdge.CornerTopLeft:
                                    case EEdge.InnerCornerTopRight:
                                    case EEdge.TopCutLeft:
                                        return EEdge.Top;
                                    case EEdge.Bottom:
                                    case EEdge.CornerBottomLeft:
                                    case EEdge.InnerCornerBottomRight:
                                    case EEdge.BottomCutLeft:
                                        return EEdge.Bottom;
                                }
                                return EEdge.None;
                            }

                            switch (edgeCached[x + 1, y + 1])
                            {
                                case EEdge.Top:
                                    {
                                        var right = ConnectionRight() == EEdge.Top;
                                        var left = ConnectionLeft() == EEdge.Top;
                                        chunk.SetData(x, y, right ? (left ? EEdge.Top : EEdge.TopCutLeft) : (left ? EEdge.TopCutRight : EEdge.TopCut));
                                    }
                                    break;
                                case EEdge.Right:
                                    {
                                        var top = ConnectionTop() == EEdge.Right;
                                        var bottom = ConnectionBottom() == EEdge.Right;
                                        chunk.SetData(x, y, top ? (bottom ? EEdge.Right : EEdge.RightCutBottom) : (bottom ? EEdge.RightCutTop : EEdge.RightCut));
                                    }
                                    break;
                                case EEdge.Bottom:
                                    {
                                        var right = ConnectionRight() == EEdge.Bottom;
                                        var left = ConnectionLeft() == EEdge.Bottom;
                                        chunk.SetData(x, y, right ? (left ? EEdge.Bottom : EEdge.BottomCutLeft) : (left ? EEdge.BottomCutRight : EEdge.BottomCut));
                                    }
                                    break;
                                case EEdge.Left:
                                    {
                                        var top = ConnectionTop() == EEdge.Left;
                                        var bottom = ConnectionBottom() == EEdge.Left;
                                        chunk.SetData(x, y, top ? (bottom ? EEdge.Left : EEdge.LeftCutBottom) : (bottom ? EEdge.LeftCutTop : EEdge.LeftCut));
                                    }
                                    break;
                                default:
                                    chunk.SetData(x, y, edgeCached[x + 1, y + 1]);
                                    break;
                            }
                        }
            }
        }





        public static void DefineUndefined(Map map)
        {
            foreach (var chunk in map.GetChunks().OrderBy(c => $"{c.MapXY.X}/{c.MapXY.Y}"))
                DefineUndefined(chunk);
        }
        public static void DefineUndefined(MapChunk chunk)
        {
            for (var x = 0; x < MapChunk.ChunkSize; x += 2)
                for (var y = 0; y < MapChunk.ChunkSize; y += 2)
                {
                    var undefinedCount = 0;
                    if (chunk.GetTile(x, y) == ETile.Undefined) ++undefinedCount;
                    if (chunk.GetTile(x + 1, y) == ETile.Undefined) ++undefinedCount;
                    if (chunk.GetTile(x, y + 1) == ETile.Undefined) ++undefinedCount;
                    if (chunk.GetTile(x + 1, y + 1) == ETile.Undefined) ++undefinedCount;

                    if (undefinedCount == 4)
                    {
                        chunk.SetTile(x, y, ETile.Tree);
                        chunk.SetTile(x + 1, y, ETile.Blocked);
                        chunk.SetTile(x, y + 1, ETile.Blocked);
                        chunk.SetTile(x + 1, y + 1, ETile.Blocked);
                    }
                    else if (undefinedCount > 0)
                    {
                        ReplaceUndefinedByTree(new Coord(x, y) + chunk.MapXY * MapChunk.ChunkSize, chunk.Map);
                    }
                }
            var undefinedCoords = PatternHelper.Rect(new Coord(), new Coord(MapChunk.ChunkSize)).ToList();
            var rand = new Random($"{chunk.MapXY}undefined{chunk.Map.ImprovementSeed}".GetHashCode());

            while (undefinedCoords.Count > 0)
            {
                var index = rand.Next(undefinedCoords.Count);
                var pos = undefinedCoords[index];
                undefinedCoords.RemoveAt(index);

                if (pos.X < 0 || pos.Y < 0 || pos.X >= MapChunk.ChunkSize || pos.Y >= MapChunk.ChunkSize) continue;
                if (chunk.GetTile(pos.X, pos.Y) != ETile.Undefined) continue;

                var tileLoop = new ETile[8];
                var rotation = new Coord(1, 0);
                for (var i = 0; i < 4; ++i)
                {
                    var cornerPos = rotation * new Coord(1, 1) + pos;
                    var edgePos = rotation + pos;
                    tileLoop[i * 2] = chunk.GetTileExtended(edgePos.X, edgePos.Y);
                    tileLoop[i * 2 + 1] = chunk.GetTileExtended(cornerPos.X, cornerPos.Y);
                    rotation *= new Coord(0, 1);
                }

                var blockingLoop = new bool?[8];
                for (var i = 0; i < 8; ++i)
                {
                    var blocking = tileLoop[i].HasBlocking();
                    var walkable = tileLoop[i].HasWalkable();
                    if (blocking != walkable)
                        blockingLoop[i] = blocking;
                }
                for (var i = 0; i < 8; i += 2)
                {
                    if (blockingLoop[i] == true)
                    {
                        blockingLoop[i + 1] = true;
                        blockingLoop[(i + 7) % 8] = true;
                    }
                }
                if (blockingLoop.Any(b => !b.HasValue))
                    continue;

                var segments = 0;
                for (var i = 0; i < 8; ++i)
                    if (blockingLoop[i] == true && blockingLoop[(i + 7) % 8] == false)
                        ++segments;
                if (segments > 1)
                    continue;

                var replacingTile = ETile.None;
                var replacingOffset = rand.Next(8);
                for (var i = 0; i < 8; ++i)
                {
                    var j = (i + replacingOffset) % 8;
                    if (blockingLoop[j] == false)
                    {
                        replacingTile = tileLoop[j];
                        break;
                    }
                }
                chunk.SetTile(pos.X, pos.Y, replacingTile);
                for (var i = 0; i < 4; ++i)
                {
                    undefinedCoords.Add(pos + rotation);
                    rotation *= new Coord(0, 1);
                }
            }

            for (var x = 0; x < MapChunk.ChunkSize; ++x)
                for (var y = 0; y < MapChunk.ChunkSize; ++y)
                    if (chunk.GetTile(x, y) == ETile.Undefined)
                        undefinedCoords.Add(new Coord(x, y));

            while (undefinedCoords.Count > 0)
            {
                var index = rand.Next(undefinedCoords.Count);
                var pos = undefinedCoords[index];
                undefinedCoords.RemoveAt(index);

                if (chunk.GetTileExtended(pos.X + 1, pos.Y).HasBlocking() &&
                    chunk.GetTileExtended(pos.X - 1, pos.Y).HasBlocking() &&
                    chunk.GetTileExtended(pos.X, pos.Y + 1).HasBlocking() &&
                    chunk.GetTileExtended(pos.X, pos.Y - 1).HasBlocking())
                    chunk.SetTile(pos.X, pos.Y, ETile.OutOfBound);
                else
                    chunk.SetTile(pos.X, pos.Y, ETile.SmallBush);
            }
        }




        public static void ReplaceUndefinedByTree(Coord corner, Map map)
        {
            var offsets = new[] { new Coord(0, 0), new Coord(1, 0), new Coord(1, 1), new Coord(0, 1) };
            var corners = new Coord[4];
            var tiles = new ETile[4];
            for (var i = 0; i < 4; ++i)
                tiles[i] = map.GetTile(corners[i] = corner + offsets[i]);

            if (tiles.Any(t => t != ETile.Undefined))
            {
                if (tiles.Any(t => !t.HasWalkable() && t != ETile.Undefined))
                    return;

                //0: bottom left corner, 1+: counter clockwise
                var rotation = new Coord(1, 0);
                var borderLoop = new bool[12];
                for (var i = 0; i < 4; ++i)
                {
                    borderLoop[i * 3] = map.GetTile(corners[i] + new Coord(-1, -1) * rotation).HasWalkable();
                    borderLoop[i * 3 + 1] = map.GetTile(corners[i] + new Coord(0, -1) * rotation).HasWalkable();
                    borderLoop[(i * 3 + 11) % 12] = map.GetTile(corners[i] + new Coord(-1, 0) * rotation).HasWalkable();
                    rotation *= new Coord(0, 1);
                }
                for (var i = 0; i < 4; ++i)
                    if (tiles[i] == ETile.Undefined)
                    {
                        borderLoop[i * 3] = false;
                        borderLoop[i * 3 + 1] = false;
                        borderLoop[(i * 3 + 11) % 12] = false;
                    }
                for (var i = 0; i < 4; ++i)
                    if (!borderLoop[i * 3 + 1] && borderLoop[(i * 3 + 11) % 12])
                        borderLoop[i * 3 + 1] = false;

                var segments = 0;
                for (var i = 0; i < 12; ++i)
                    if (borderLoop[i] && !borderLoop[(i + 11) % 12])
                        ++segments;

                if (tiles[0] == tiles[2] &&
                    tiles[1] == tiles[3] &&
                    tiles[0] != tiles[1])
                    --segments;

                if (segments > 1)
                    return;
            }
            map.SetTile(corner, ETile.Tree);
            for (var i = 1; i < 4; ++i)
                map.SetTile(corners[i], ETile.Blocked);
        }
    }
}
