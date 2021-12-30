using MapEngine;

namespace MapGameplay
{
    public class COrientation : Component
    {
        public enum EDirection { Forward, Right, Backward, Left }
        public EDirection Direction;
    }
}
