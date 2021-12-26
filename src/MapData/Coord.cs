using System;
using System.Linq;
using Math = UnhedderEngine.Math;

namespace MapData
{
    public struct Coord
    {
        public Coord(int xy) : this(xy, xy) { }
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X, Y;
        public static Coord operator +(Coord left, Coord right) => new Coord(left.X + right.X, left.Y + right.Y);
        public static Coord operator -(Coord left, Coord right) => new Coord(left.X - right.X, left.Y - right.Y);
        /// <summary>
        /// Complex Number Multiplication
        /// <returns></returns>
        public static Coord operator *(Coord left, Coord right) => new Coord(left.X * right.X - left.Y * right.Y, left.X * right.Y + left.Y * right.X);
        public Coord Scale(Coord other) => new Coord(X * other.X, Y * other.Y);
        public static Coord operator *(Coord left, int right) => new Coord(left.X * right, left.Y * right);
        public static Coord operator *(int left, Coord right) => new Coord(left * right.X, left * right.Y);
        public static Coord operator /(Coord left, int right) => new Coord(left.X / right, left.Y / right);

        public static bool operator ==(Coord left, Coord right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(Coord left, Coord right) => !(left == right);
        public override bool Equals(object obj) => (obj is Coord coord) && this == coord;

        public static bool operator >(Coord left, Coord right) => left.X > right.X && left.Y > right.Y;
        public static bool operator >=(Coord left, Coord right) => left.X >= right.X && left.Y >= right.Y;
        public static bool operator <(Coord left, Coord right) => left.X < right.X && left.Y < right.Y;
        public static bool operator <=(Coord left, Coord right) => left.X <= right.X && left.Y <= right.Y;

        public Coord Abs() => new Coord(Math.Abs(X), Math.Abs(Y));
        public Coord T() => new Coord(Y, X);

        public static Coord Min(params Coord[] xy) => new Coord(Math.Min(xy.Select(v => v.X)), Math.Min(xy.Select(v => v.Y)));
        public static Coord Max(params Coord[] xy) => new Coord(Math.Max(xy.Select(v => v.X)), Math.Max(xy.Select(v => v.Y)));
        public override string ToString() => $"({X}{(Y < 0 ? "" : "+")}{Y}i)";
        public override int GetHashCode() => Tuple.Create(X, Y).GetHashCode();
    }
}
