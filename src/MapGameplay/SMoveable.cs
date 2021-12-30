using MapData;
using MapData.Tile;
using MapEngine;
using UnhedderEngine;
using UnhedderEngine.Input;

namespace MapGameplay
{
    [Priority(1)]
    public class SMoveable : MapSystem
    {
        public override void OnUpdate(FrameData data)
        {
            var moveables = World.GetComponents<CMoveable>();
            foreach (var moveable in moveables)
            {
                var sqr = moveable.Offset.SqrLength;
                if (sqr == 0)
                {
                    if (moveable.NextMove == default)
                        continue;
                    ChangePosition(moveable);
                    sqr = moveable.Offset.SqrLength;
                    if (sqr == 0)
                        continue;
                }
                var availableDistance = data.DeltaTime * moveable.Speed;
                if (availableDistance * availableDistance >= sqr)
                {
                    if (moveable.NextMove != default)
                    {
                        ChangePosition(moveable);
                        if (availableDistance * availableDistance >= moveable.Offset.SqrLength)
                            UpdateOffset(moveable, Vec3.Zero);
                        else
                        {
                            var length = moveable.Offset.Length;
                            UpdateOffset(moveable, moveable.Offset * (length - availableDistance) / length);
                        }
                    }
                    else
                        UpdateOffset(moveable, Vec3.Zero);
                }
                else
                {
                    var length = moveable.Offset.Length;
                    UpdateOffset(moveable, moveable.Offset * (length - availableDistance) / length);
                }
            }
        }

        private static void UpdateOffset(CMoveable moveable, Vec3 offset)
        {
            moveable.Entity.VisualOffset = moveable.Offset = offset;
        }


        private void ChangePosition(CMoveable moveable)
        {
            if (World.LogicPaused)
            {
                moveable.NextMove = default;
                return;
            }
            var orientation = moveable.Entity.Get<COrientation>();
            if (orientation != null)
            {
                if (moveable.NextMove.X > 0)
                    orientation.Direction = COrientation.EDirection.Right;
                else if (moveable.NextMove.X < 0)
                    orientation.Direction = COrientation.EDirection.Left;
                else if (moveable.NextMove.Y > 0)
                    orientation.Direction = COrientation.EDirection.Forward;
                else if (moveable.NextMove.Y < 0)
                    orientation.Direction = COrientation.EDirection.Backward;
            }
            var newPosition = moveable.Entity.Position + moveable.NextMove;
            if (!moveable.Blockable || !IsPositionBlocked(moveable, newPosition))
            {
                moveable.Entity.Position = newPosition;
                moveable.Offset += new Vec3(-moveable.NextMove.X, 0, moveable.NextMove.Y);
            }
            moveable.NextMove = default;
        }

        private static bool IsPositionBlocked(CMoveable moveable, Coord pos)
        {
            var map = moveable.Entity.Map;
            if (map == null) return true;
            var field = map.GetTile(pos);
            if (field.HasWalkable())
                return false;
            if (field.HasBlocking())
                return true;

            switch (field)
            {
                default:
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    return true;
            }
        }
    }
}
