using System;
using UnhedderEngine.Input;

namespace MapEngine
{
    public abstract class MapSystem
    {
        public World World { get; internal set; }

        [ThreadStatic]
        internal static bool _allowConstructor;
        public MapSystem()
        {
            if (!_allowConstructor)
                throw new NotSupportedException($"{GetType().Name} must be created by {nameof(MapEngine.World)}.{nameof(MapEngine.World.AddSystem)}");
        }
        public virtual void OnUpdate(FrameData data) { }
    }
}
