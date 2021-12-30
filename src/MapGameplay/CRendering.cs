using MapEngine;
using System.Collections.Generic;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;

namespace MapGameplay
{
    public abstract class CRendering : Component
    {
        internal readonly List<(SingleRenderer Renderer, Vec3 Offset)> Renderers = new List<(SingleRenderer Renderer, Vec3 Offset)>();
    }
}
