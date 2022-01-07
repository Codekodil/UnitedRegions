using MapData;
using MonsterData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;

namespace MapEngine
{
    public sealed class World : IDisposable
    {
        private readonly object _locker = new object();

        public event Func<bool> ShouldPauseLogic;
        public bool LogicPaused { get; private set; }

        public MonsterBox MonsterTeam { get; } = new MonsterBox(6, true);

        public World()
        {
            EventManager.Update += OnUpdate;
        }
        private void OnUpdate(FrameData data)
        {
            try
            {
                LogicPaused = false;
                var invokationList = ShouldPauseLogic?.GetInvocationList();
                if (invokationList != null)
                    foreach (var v in invokationList)
                        if (v.DynamicInvoke() is bool b && b)
                            LogicPaused = true;

                foreach (var system in _systems.ToArray())
                    system.OnUpdate(data);
            }
            catch (Exception)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#else
                throw;
#endif
            }
        }

        private readonly Dictionary<string, Map> _maps = new Dictionary<string, Map>();
        private readonly Dictionary<Map, Scene> _scenes = new Dictionary<Map, Scene>();
        private readonly List<MapSystem> _systems = new List<MapSystem>();
        private readonly List<MapEntity> _entities = new List<MapEntity>();
        private readonly Dictionary<Type, object> _components = new Dictionary<Type, object>();
        private readonly Dictionary<Type, List<(MapSystem System, MethodInfo Method)>> _onAddComponents = new Dictionary<Type, List<(MapSystem System, MethodInfo Method)>>();
        private readonly Dictionary<Type, List<(MapSystem System, MethodInfo Method)>> _onRemoveComponents = new Dictionary<Type, List<(MapSystem System, MethodInfo Method)>>();
        private readonly Dictionary<Type, List<(MapSystem System, MethodInfo Method)>> _beforeMapChange = new Dictionary<Type, List<(MapSystem System, MethodInfo Method)>>();
        private readonly Dictionary<Type, List<(MapSystem System, MethodInfo Method)>> _afterMapChange = new Dictionary<Type, List<(MapSystem System, MethodInfo Method)>>();
        public MapEntity[] Entities
        {
            get
            {
                lock (_locker)
                    return _entities.ToArray();
            }
        }

        public void AddMap(string id, Map map)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (map == null) throw new ArgumentNullException(nameof(map));
            lock (_locker)
            {
                if (Disposed) throw new ObjectDisposedException(nameof(World));
                if (_maps.ContainsKey(id))
                    throw new ArgumentException($"Map {id} already exists");
                if (_scenes.ContainsKey(map))
                    throw new ArgumentException($"Map is already a part of this World");
                _maps[id] = map;
                _scenes[map] = new Scene();
            }
        }
        public Map GetMap(string id)
        {
            lock (_locker)
            {
                if (!_maps.ContainsKey(id))
                    throw new ArgumentException($"invalid Map id {id}");
                return _maps[id];
            }
        }
        public Scene GetScene(string id)
        {
            lock (_locker)
            {
                if (!_maps.ContainsKey(id))
                    throw new ArgumentException($"invalid Map id {id}");
                return _scenes[_maps[id]];
            }
        }
        public Scene GetScene(Map map)
        {
            lock (_locker)
            {
                if (!_scenes.ContainsKey(map))
                    throw new ArgumentException($"unknown Map");
                return _scenes[map];
            }
        }

        public T[] GetComponents<T>() where T : Component
        {
            lock (_locker)
                return _components.TryGetValue(typeof(T), out var list) ?
                    ((List<T>)list).ToArray() :
                    new T[0];
        }

        public MapEntity NewEntity(string mapId) => NewEntity(GetMap(mapId));
        public MapEntity NewEntity(Map map)
        {
            lock (_locker)
            {
                if (Disposed) throw new ObjectDisposedException(nameof(World));
                if (!_scenes.ContainsKey(map))
                    throw new ArgumentException($"unknown Map");
                var result = new MapEntity(this);
                result._map = map;
                _entities.Add(result);
                return result;
            }
        }
        internal void ChangeMap(MapEntity entity, Map map)
        {
            var before = new List<(object Component, List<(MapSystem System, MethodInfo Method)> Methods)>();
            var after = new List<(object Component, List<(MapSystem System, MethodInfo Method)> Methods)>();
            var components = entity.Components;
            lock (_locker)
            {
                foreach (var component in components)
                {
                    if (_beforeMapChange.TryGetValue(component.GetType(), out var beforeMethods))
                        before.Add((component, beforeMethods));
                    if (_afterMapChange.TryGetValue(component.GetType(), out var afterMethods))
                        after.Add((component, afterMethods));
                }
            }
            foreach (var component in before)
                foreach (var method in component.Methods)
                    method.Method.Invoke(method.System, new object[] { component.Component });
            entity._map = map;
            foreach (var component in after)
                foreach (var method in component.Methods)
                    method.Method.Invoke(method.System, new object[] { component.Component });
        }
        public void AddSystem<T>() where T : MapSystem => AddSystem(typeof(T));
        public void AddSystem(Type type)
        {
            if (!typeof(MapSystem).IsAssignableFrom(type))
                throw new ArgumentException($"{type} is not a {nameof(MapSystem)}");
            lock (_locker)
            {
                if (Disposed) throw new ObjectDisposedException(nameof(World));
                try
                {
                    MapSystem._allowConstructor = true;
                    var system = (MapSystem)Activator.CreateInstance(type);
                    system.World = this;

                    foreach (var method in type.GetMethods())
                    {
                        if (!method.IsPublic || method.IsStatic) continue;
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 ||
                            !typeof(Component).IsAssignableFrom(parameters[0].ParameterType) &&
                            parameters[0].IsIn &&
                            parameters[0].IsOut)
                            continue;

                        if (method.GetCustomAttribute<OnAddComponentAttribute>() != null) AddMethod(_onAddComponents);
                        if (method.GetCustomAttribute<OnRemoveComponentAttribute>() != null) AddMethod(_onRemoveComponents);
                        if (method.GetCustomAttribute<BeforeMapChangeAttribute>() != null) AddMethod(_beforeMapChange);
                        if (method.GetCustomAttribute<AfterMapChangeAttribute>() != null) AddMethod(_afterMapChange);

                        void AddMethod(Dictionary<Type, List<(MapSystem System, MethodInfo Method)>> eventMethods)
                        {
                            //creating a new list because the current might be accessed outside a lock
                            eventMethods[parameters[0].ParameterType] =
                                (eventMethods.TryGetValue(parameters[0].ParameterType, out var list) ? list : new List<(MapSystem System, MethodInfo Method)>())
                                .Concat(new (MapSystem System, MethodInfo Method)[] { (system, method) })
                                .ToList();
                        }
                    }
                    _systems.Add(system);
                    _systems.Sort((l, r) =>
                    {
                        var priority = GetPriority(r) - GetPriority(l);
                        return priority < 0 ? -1 : (priority > 0 ? 1 : 0);
                    });
                    float GetPriority(MapSystem s) => s.GetType().GetCustomAttribute<PriorityAttribute>()?.Priority ?? 0;
                }
                finally
                {
                    MapSystem._allowConstructor = false;
                }
            }
        }
        public T GetSystem<T>() where T : MapSystem
        {
            lock (_locker)
            {
                var result = _systems.OfType<T>().FirstOrDefault();
                if (result == null)
                    throw new ArgumentException($"unknown system of type {typeof(T)}");
                return result;
            }
        }

        internal void RemoveObject(MapEntity entity)
        {
            lock (_locker)
                _entities.Remove(entity);
        }

        internal void AddComponent<T>(T component)
        {
            List<(MapSystem System, MethodInfo Method)> eventMethods = null;
            lock (_locker)
            {
                List<T> list;
                if (_components.TryGetValue(typeof(T), out var obj))
                    list = (List<T>)obj;
                else
                    _components[typeof(T)] = list = new List<T>();
                list.Add(component);
                if (_onAddComponents.TryGetValue(typeof(T), out var e))
                    eventMethods = e;
            }
            if (eventMethods != null)
                foreach (var method in eventMethods)
                    method.Method.Invoke(method.System, new object[] { component });
        }
        internal void RemoveComponent(Component component)
        {
            List<(MapSystem System, MethodInfo Method)> eventMethods = null;
            lock (_locker)
            {
                if (!_components.TryGetValue(component.GetType(), out var obj)) return;
                ((IList)obj).Remove(component);
                if (_onRemoveComponents.TryGetValue(component.GetType(), out var e))
                    eventMethods = e;
            }
            if (eventMethods != null)
                foreach (var method in eventMethods)
                    method.Method.Invoke(method.System, new object[] { component });
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
