using MapEngine;
using System.Collections.Generic;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;

namespace MapGameplay
{
    public class CRendering : Component
    {
        public readonly List<(SingleRenderer Renderer, Vec3 Offset)> Renderers = new List<(SingleRenderer Renderer, Vec3 Offset)>();
    }
}
