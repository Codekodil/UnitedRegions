using MapData;
using MapEngine;
using System.Linq;
using UnhedderEngine;
using UnhedderEngine.Input;

namespace MapGameplay
{
    public class SCameraManager : MapSystem
    {
        public Vec3 CameraOffset { get; set; }

        public Camera GetCamera(string mapId) => GetCamera(World.GetMap(mapId));
        public Camera GetCamera(Map map)
        {
            var cameraFocus = World.GetComponents<CCameraFocus>().FirstOrDefault(f => f.Entity.Map == map);
            if (cameraFocus == null)
            {
                var entity = World.NewEntity(map);
                cameraFocus = entity.Add<CCameraFocus>();
                var player = World.GetComponents<CPlayer>().FirstOrDefault(p => p.Entity.Map == map);
                if (player != null)
                {
                    cameraFocus.CurrentFocus = player.Entity;
                    cameraFocus.Entity.Position = player.Entity.Position;
                    cameraFocus.LastCenter = player.Entity.CenterVec;
                }
            }
            SetCameraTransform(cameraFocus);
            return cameraFocus.Camera;
        }

        public override void OnUpdate(FrameData data)
        {
            var cameraFoci = World.GetComponents<CCameraFocus>();
            var players = World.GetComponents<CPlayer>();
            foreach (var focus in cameraFoci)
            {
                if (focus.CurrentFocus?.Disposed == true)
                    focus.CurrentFocus = null;
                if (focus.CurrentFocus?.Get<CPlayer>() == null)
                {
                    var playerInSameMap = players.FirstOrDefault(p => p.Entity.Map == focus.Entity.Map);
                    if (playerInSameMap != null)
                        focus.CurrentFocus = playerInSameMap.Entity;
                }
                if (focus.CurrentFocus != null)
                {
                    focus.Entity.Position = focus.CurrentFocus.Position;
                    focus.LastCenter = focus.CurrentFocus.CenterVec;
                    SetCameraTransform(focus);
                }
            }
        }

        private void SetCameraTransform(CCameraFocus focus)
        {
            if (focus.Camera == null)
            {
                focus.Camera = new Camera { Scene = World.GetScene(focus.Entity.Map) };
                focus.Camera.SetFoV(Math.Pi * .15f);
            }
            focus.Camera.Position = focus.LastCenter + CameraOffset;
            focus.Camera.Rotation = Quaternion.LookAt(-CameraOffset, Vec3.Up);
        }
    }
}
