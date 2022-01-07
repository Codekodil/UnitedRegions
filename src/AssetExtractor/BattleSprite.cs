using UnhedderEngine;

namespace AssetExtractor
{
    public class BattleSprite
    {
        public Texture Texture { get; }
        public Vec2 MinBoundingBox { get; }
        public Vec2 MaxBoundingBox { get; }

        internal BattleSprite(Texture texture, Vec2 min, Vec2 max)
        {
            Texture = texture;
            MinBoundingBox = min;
            MaxBoundingBox = max;
        }
    }
}
