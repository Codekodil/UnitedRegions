using MapEngine;
using UnhedderEngine;
using UnhedderEngine.Input;

namespace MapGameplay
{
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
                        return;
                    ChangePosition(moveable);
                    moveable.NextMove = default;
                    sqr = moveable.Offset.SqrLength;
                }
                var availableDistance = data.DeltaTime * moveable.Speed;
                if (availableDistance * availableDistance >= sqr)
                {
                    if (moveable.NextMove != default)
                    {
                        ChangePosition(moveable);
                        moveable.NextMove = default;
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


        private static void ChangePosition(CMoveable moveable)
        {
            moveable.Entity.Position += moveable.NextMove;
            moveable.Offset += new Vec3(-moveable.NextMove.X, 0, moveable.NextMove.Y);
        }
    }
}
