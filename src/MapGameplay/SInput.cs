using MapData;
using MapEngine;
using System.Collections.Generic;
using UnhedderEngine.Input;

namespace MapGameplay
{
    public class SInput : MapSystem
    {
        private readonly HashSet<string> _keys = new HashSet<string>();
        public void KeyDown(string key)
        {
            lock (_keys)
                _keys.Add(key);
        }
        public void KeyUp(string key)
        {
            lock (_keys)
                _keys.Remove(key);
        }

        public override void OnUpdate(FrameData data)
        {
            if (World.LogicPaused) return;

            var moving = World.GetComponents<CMovedByInput>();
            foreach (var move in moving)
            {
                var moveable = move.Entity.Get<CMoveable>();
                if (moveable == null) continue;

                Coord direction;
                lock (_keys)
                    direction = new Coord(
                            (_keys.Contains("A") ? -1 : 0) + (_keys.Contains("D") ? 1 : 0),
                            (_keys.Contains("S") ? -1 : 0) + (_keys.Contains("W") ? 1 : 0));
                if (direction.Y != 0) direction.X = 0;
                moveable.NextMove = direction;
            }
        }
    }
}
