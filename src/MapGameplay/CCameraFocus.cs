using MapEngine;
using UnhedderEngine;

namespace MapGameplay
{
    public class CCameraFocus : Component
    {
        public MapEntity CurrentFocus;
        public Vec3 LastCenter;
        public Camera Camera;
    }
}
