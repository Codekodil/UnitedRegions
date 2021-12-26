using System;

namespace MapEngine
{
    public abstract class Component : IDisposable
    {
        private readonly object _locker = new object();
        public MapEntity Entity { get; internal set; }

        [ThreadStatic]
        internal static bool _allowConstructor;
        public Component()
        {
            if (!_allowConstructor)
                throw new NotSupportedException($"{GetType().Name} must be created by {nameof(MapEntity)}.{nameof(MapEntity.Add)}");
        }

        public bool Disposed { get; private set; }
        public void Dispose()
        {
            lock (_locker)
            {
                if (Disposed) return;
                Disposed = true;
                Entity.Remove(this);
            }
            Entity = null;
        }
    }
}
