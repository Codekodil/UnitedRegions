using MapData;
using MapEngine;
using UnhedderEngine;

namespace MapGameplay
{
    public class CMoveable : Component
    {
        public float Speed = 1;
        public Vec3 Offset;
        public Coord NextMove;
    }
}
