using System;
using System.Collections;
using System.Collections.Generic;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;

namespace MapEngine
{
    public sealed class World : IDisposable
    {
        private readonly object _locker = new object();

        public World()
        {
            EventManager.Update += OnUpdate;
        }
        private void OnUpdate(FrameData data)
        {
            foreach (var system in _systems.ToArray())
                system.OnUpdate(data);
        }

        public Scene Scene { get; } = new Scene();

        private readonly List<MapSystem> _systems = new List<MapSystem>();
        private readonly List<MapEntity> _entities = new List<MapEntity>();
        private readonly Dictionary<Type, object> _components = new Dictionary<Type, object>();
        public MapEntity[] Entities
        {
            get
            {
                lock (_locker)
                    return _entities.ToArray();
            }
        }

        public T[] GetComponents<T>()
        {
            lock (_locker)
                return _components.TryGetValue(typeof(T), out var list) ?
                    ((List<T>)list).ToArray() :
                    new T[0];
        }

        public MapEntity NewEntity()
        {
            lock (_locker)
            {
                if (Disposed) return null;
                var result = new MapEntity(this);
                _entities.Add(result);
                return result;
            }
        }
        public void AddSystem<T>() where T : MapSystem
        {
            lock (_locker)
            {
                try
                {
                    MapSystem._allowConstructor = true;
                    var system = Activator.CreateInstance<T>();
                    system.World = this;
                    _systems.Add(system);
                }
                finally
                {
                    MapSystem._allowConstructor = false;
                }
            }
        }

        internal void RemoveObject(MapEntity entity)
        {
            lock (_locker)
                _entities.Remove(entity);
        }

        internal void AddComponent<T>(T component)
        {
            lock (_locker)
            {
                List<T> list;
                if (_components.TryGetValue(typeof(T), out var obj))
                    list = (List<T>)obj;
                else
                    _components[typeof(T)] = list = new List<T>();
                list.Add(component);
            }
        }
        internal void RemoveComponent(Component component)
        {
            lock (_locker)
            {
                if (!_components.TryGetValue(component.GetType(), out var obj)) return;
                ((IList)obj).Remove(component);
            }
        }

        public bool Disposed { get; private set; }
        public void Dispose()
        {
            MapEntity[] entities;
            lock (_locker)
            {
                if (Disposed) return;
                Disposed = true;
                EventManager.Update -= OnUpdate;
                entities = Entities;
                _entities.Clear();
            }
            foreach (var entity in entities)
                entity.Dispose();
        }
    }
}
