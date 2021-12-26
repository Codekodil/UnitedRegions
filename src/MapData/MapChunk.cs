namespace MapData
{
    public class MapChunk
    {
        public MapChunk(int x, int y, Map map) : this(new Coord(x, y), map) { }
        public MapChunk(Coord pos, Map map)
        {
            MapXY = pos;
            Map = map;
        }
        public Coord MapXY { get; }
        public Map Map { get; }

        public const int ChunkSize = 32;

        private readonly ETile[] _tiles = new ETile[ChunkSize * ChunkSize];
        private readonly EGround[] _grounds = new EGround[ChunkSize * ChunkSize];
        private readonly EEdge[] _edge = new EEdge[ChunkSize * ChunkSize];
        private readonly int[] _height = new int[ChunkSize * ChunkSize];
        private readonly object[] _data = new object[ChunkSize * ChunkSize];

        public ETile GetTile(int x, int y) => _tiles[x + y * ChunkSize];
        public ETile GetTileExtended(int x, int y) => (x >= 0 && y >= 0 && x < ChunkSize && y < ChunkSize) ? GetTile(x, y) : Map.GetTile(new Coord(x, y) + MapXY * ChunkSize);
        public void SetTile(int x, int y, ETile tile) => _tiles[x + y * ChunkSize] = tile;
        public EGround GetGround(int x, int y) => _grounds[x + y * ChunkSize];
        public EGround GetGroundExtended(int x, int y) => (x >= 0 && y >= 0 && x < ChunkSize && y < ChunkSize) ? GetGround(x, y) : Map.GetGround(new Coord(x, y) + MapXY * ChunkSize);
        public void SetGround(int x, int y, EGround ground) => _grounds[x + y * ChunkSize] = ground;
        public EEdge GetEdge(int x, int y) => _edge[x + y * ChunkSize];
        public EEdge GetEdgeExtended(int x, int y) => (x >= 0 && y >= 0 && x < ChunkSize && y < ChunkSize) ? GetEdge(x, y) : Map.GetEdge(new Coord(x, y) + MapXY * ChunkSize);
        public void SetEdge(int x, int y, EEdge edge) => _edge[x + y * ChunkSize] = edge;
        public int GetHeight(int x, int y) => _height[x + y * ChunkSize];
        public int GetHeightExtended(int x, int y) => (x >= 0 && y >= 0 && x < ChunkSize && y < ChunkSize) ? GetHeight(x, y) : Map.GetHeight(new Coord(x, y) + MapXY * ChunkSize);
        public void SetHeight(int x, int y, int height) => _height[x + y * ChunkSize] = height;
        public object GetData(int x, int y) => _data[x + y * ChunkSize];
        public object GetDataExtended(int x, int y) => (x >= 0 && y >= 0 && x < ChunkSize && y < ChunkSize) ? GetData(x, y) : Map.GetData(new Coord(x, y) + MapXY * ChunkSize);
        public void SetData(int x, int y, object data) => _data[x + y * ChunkSize] = data;
    }
}
