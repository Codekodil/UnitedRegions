using System.Collections.Generic;

namespace MapData
{
    public class Map
    {
        private readonly Dictionary<Coord, MapChunk> _chunks = new Dictionary<Coord, MapChunk>();

        public int ImprovementSeed { get; }
        public Map(int improvementSeed) => ImprovementSeed = improvementSeed;

        public MapChunk[] GetChunks()
        {
            lock (_chunks)
            {
                var result = new MapChunk[_chunks.Count];
                _chunks.Values.CopyTo(result, 0);
                return result;
            }
        }

        public MapChunk GetChunk(int x, int y) => GetChunk(new Coord(x, y));
        public MapChunk GetChunk(Coord pos)
        {
            lock (_chunks)
            {
                if (!_chunks.TryGetValue(pos, out var chunk))
                    _chunks[pos] = chunk = new MapChunk(pos, this);
                return chunk;
            }
        }
        public MapChunk GetChunkNullable(int x, int y) => GetChunkNullable(new Coord(x, y));
        public MapChunk GetChunkNullable(Coord pos)
        {
            lock (_chunks)
                return _chunks.TryGetValue(pos, out var chunk) ? chunk : null;
        }

        public static int ChunkCoord(int a) => a < 0 ? a / MapChunk.ChunkSize - 1 : a / MapChunk.ChunkSize;
        public static int TileCoord(int a) => a < 0 ? a % MapChunk.ChunkSize + MapChunk.ChunkSize : a % MapChunk.ChunkSize;

        public ETile GetTile(Coord pos) => GetTile(pos.X, pos.Y);
        public ETile GetTile(int x, int y) => GetChunkNullable(ChunkCoord(x), ChunkCoord(y))?.GetTile(TileCoord(x), TileCoord(y)) ?? default;
        public void SetTile(Coord pos, ETile tile) => SetTile(pos.X, pos.Y, tile);
        public void SetTile(int x, int y, ETile tile) => GetChunk(ChunkCoord(x), ChunkCoord(y)).SetTile(TileCoord(x), TileCoord(y), tile);
        public EGround GetGround(Coord pos) => GetGround(pos.X, pos.Y);
        public EGround GetGround(int x, int y) => GetChunkNullable(ChunkCoord(x), ChunkCoord(y))?.GetGround(TileCoord(x), TileCoord(y)) ?? default;
        public void SetGround(Coord pos, EGround ground) => SetGround(pos.X, pos.Y, ground);
        public void SetGround(int x, int y, EGround ground) => GetChunk(ChunkCoord(x), ChunkCoord(y)).SetGround(TileCoord(x), TileCoord(y), ground);
        public EEdge GetEdge(Coord pos) => GetEdge(pos.X, pos.Y);
        public EEdge GetEdge(int x, int y) => GetChunkNullable(ChunkCoord(x), ChunkCoord(y))?.GetEdge(TileCoord(x), TileCoord(y)) ?? default;
        public void SetEdge(Coord pos, EEdge edge) => SetEdge(pos.X, pos.Y, edge);
        public void SetEdge(int x, int y, EEdge edge) => GetChunk(ChunkCoord(x), ChunkCoord(y)).SetEdge(TileCoord(x), TileCoord(y), edge);
        public int GetHeight(Coord pos) => GetHeight(pos.X, pos.Y);
        public int GetHeight(int x, int y) => GetChunkNullable(ChunkCoord(x), ChunkCoord(y))?.GetHeight(TileCoord(x), TileCoord(y)) ?? default;
        public void SetHeight(Coord pos, int height) => SetHeight(pos.X, pos.Y, height);
        public void SetHeight(int x, int y, int height) => GetChunk(ChunkCoord(x), ChunkCoord(y)).SetHeight(TileCoord(x), TileCoord(y), height);
        public object GetData(Coord pos) => GetData(pos.X, pos.Y);
        public object GetData(int x, int y) => GetChunkNullable(ChunkCoord(x), ChunkCoord(y))?.GetData(TileCoord(x), TileCoord(y));
        public void SetData(Coord pos, object data) => SetData(pos.X, pos.Y, data);
        public void SetData(int x, int y, object data) => GetChunk(ChunkCoord(x), ChunkCoord(y)).SetData(TileCoord(x), TileCoord(y), data);

    }
}
