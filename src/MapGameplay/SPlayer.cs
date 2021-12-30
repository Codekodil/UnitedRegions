using MapData;
using MapEngine;
using System;
using System.Linq;
using UnhedderEngine.Input;

namespace MapGameplay
{
    public class SPlayer : MapSystem
    {
        [OnAddComponent]
        public void CheckForMultiplePlayers(CPlayer player)
        {
            var players = World.GetComponents<CPlayer>();
            if (players.Length > 1)
                player.Dispose();
        }

        public event Action<CPlayer> StepOntoTallGrass;

        private Coord _lastPosition;

        public override void OnUpdate(FrameData data)
        {
            var players = World.GetComponents<CPlayer>();
            if (players.Length == 0) return;

            foreach (var v in players.Skip(2))
                v.Dispose();

            var player = players[0];
            if (player.Entity.Position != _lastPosition)
            {
                _lastPosition = player.Entity.Position;

                var tile = player.Entity.Map.GetTile(_lastPosition);

                switch (tile)
                {
                    case ETile.TallGrass:
                        StepOntoTallGrass?.Invoke(player);
                        break;
                }
            }
        }
    }
}
