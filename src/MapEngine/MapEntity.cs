using MapData;
using System;
using System.Collections.Generic;
using UnhedderEngine;

namespace MapEngine
{
    public sealed class MapEntity : IDisposable
    {
        private readonly object _locker = new object();

        public World World { get; private set; }
        internal MapEntity(World world) => World = world;
        internal Map _map;
        public Map Map
        {
            get => _map;
            set
            {

            }
        }


        public Coord Position { get; set; }
        public int Height { get; set; }
        public Coord Size { get; set; } = new Coord(1);
        public Vec3 VisualOffset { get; set; }

        public Vec3 CenterVec => new Vec3(Position.X + Size.X * .5f, Height, -Position.Y - Size.Y * .5f) + VisualOffset;

        public bool Solid { get; set; }

        private readonly Dictionary<Type, Component> _components = new Dictionary<Type, Component>();
        public Component[] Components
        {
            get
            {
                lock (_locker)
                {
                    var result = new Component[_components.Count];
                    _components.Values.CopyTo(result, 0);
                    return result;
                }
            }
        }

        public T Get<T>() where T : Component
        {
            lock (_locker)
                return _components.TryGetValue(typeof(T), out var component) ? (T)component : null;
        }
        public T Add<T>() where T : Component
        {
            lock (_locker)
            {
                if (Disposed) return null;
                if (_components.ContainsKey(typeof(T))) throw new ArgumentException($"entity already has component {typeof(T)}");

                if (typeof(T).IsAbstract)
                    throw new ArgumentException($"type {typeof(T)} is abstract");

                T component;
                try
                {
                    Component._allowConstructor = true;
                    component = Activator.CreateInstance<T>();
                }
                finally
                {
                    Component._allowConstructor = false;
                }
                component.Entity = this;
                _components[typeof(T)] = component;
                World.AddComponent(component);
                return component;
            }
        }
        public T Force<T>() where T : Component
        {
            lock (_locker)
                return Get<T>() ?? Add<T>();
        }
        internal void Remove(Component component)
        {
            lock (_locker)
            {
                if (_components.TryGetValue(component.GetType(), out var current) && current == component)
                    _components.Remove(component.GetType());
                World.RemoveComponent(component);
            }
        }

        public bool Disposed { get; private set; }
        public void Dispose()
        {
            Component[] components;
            lock (_locker)
            {
                if (Disposed) return;
                Disposed = true;
                World.RemoveObject(this);
                components = Components;
                _components.Clear();
            }
            foreach (var component in components)
                component.Dispose();
            World = null;
        }
    }
}
