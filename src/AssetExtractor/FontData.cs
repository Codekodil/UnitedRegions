using System.Collections.Generic;
using UnhedderEngine;

namespace AssetExtractor
{
    public class FontData
    {
        public Texture Texture { get; }
        public Dictionary<char, (Vec2 UvMin, Vec2 UvMax, float TotalWidth, float Offset, float SymbolWidth)> SymbolData { get; }
        public FontData(Texture texture, Dictionary<char, (Vec2 UvMin, Vec2 UxMax, float TotalWidth, float Offset, float SymbolWidth)> symbolData)
        {
            Texture = texture;
            SymbolData = symbolData;
        }
    }
}
