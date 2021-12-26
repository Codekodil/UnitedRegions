using MapData;
using System;
using System.Collections.Generic;

namespace MapGenerator
{
    internal static class PatternHelper
    {
        public static IEnumerable<Coord> BoundingRect(params Coord[] insideCoord)
        {
            var min = Coord.Min(insideCoord);
            var max = Coord.Max(insideCoord);
            return Rect(min, max - min + new Coord(1));
        }
        public static IEnumerable<Coord> Rect(Coord insideDownLeft, Coord insideSize) => Rect(insideDownLeft.X, insideDownLeft.Y, insideSize.X, insideSize.Y);
        public static IEnumerable<Coord> Rect(int insideLeft, int insideDown, int insideWidth, int insideHeight)
        {
            for (var x = 0; x < insideWidth; ++x)
                for (var y = 0; y < insideHeight; ++y)
                    yield return new Coord(x + insideLeft, y + insideDown);
        }
        public static IEnumerable<Coord> Border(Coord insideDownLeft, Coord insideSize, int borderWidth = 1) => Border(insideDownLeft.X, insideDownLeft.Y, insideSize.X, insideSize.Y, borderWidth);
        public static IEnumerable<Coord> Border(int insideLeft, int insideDown, int insideWidth, int insideHeight, int borderWidth = 1)
        {
            for (var b = 0; b < borderWidth; ++b)
            {
                for (var x = -borderWidth; x < insideWidth; ++x)
                {
                    yield return new Coord(x + insideLeft, insideDown - b - 1);
                    yield return new Coord(x + insideLeft + borderWidth, insideDown + insideHeight + b);
                }
                for (var y = -borderWidth; y < insideHeight; ++y)
                {
                    yield return new Coord(insideLeft - b - 1, y + insideDown + borderWidth);
                    yield return new Coord(insideLeft + insideWidth + b, y + insideDown);
                }
            }
        }

        public static T RandomElement<T>(Random rand, params T[] elements) => elements[rand.Next(elements.Length)];

    }
}
