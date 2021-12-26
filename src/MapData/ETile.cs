using MapData.Tile;
using System;
using System.Linq;

namespace MapData
{
    public enum ETile
    {
        [Blocking] Undefined,
        [Blocking] Blocked,
        [Blocking] OutOfBound,

        [Walkable] None,
        [Walkable] TallGrass,

        [Blocking] Tree,
        SmallCliffs,
        HealCenter,
        Shop,
        House,
        [Blocking] SmallBush
    }

}

namespace MapData.Tile
{
    public class WalkableAttribute : Attribute { }
    public class BlockingAttribute : Attribute { }
    public static class ETileExtensions
    {
        public static bool HasWalkable(this ETile tile) => Check<WalkableAttribute>(tile);
        public static bool HasBlocking(this ETile tile) => Check<BlockingAttribute>(tile);
        private static bool Check<T>(ETile tile) where T : Attribute =>
            typeof(ETile).GetField(tile.ToString()).GetCustomAttributes(typeof(T), false).Any();
    }
}
