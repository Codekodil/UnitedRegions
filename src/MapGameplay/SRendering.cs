using Gen4Assets;
using MapEngine;
using UnhedderEngine;
using UnhedderEngine.Input;
using UnhedderEngine.Workflows.Core;
using static UnhedderEngine.Mesh;

namespace MapGameplay
{
    public class SRendering : MapSystem
    {
        private static Mesh _spriteMesh;
        static SRendering()
        {
            _spriteMesh = new Mesh(false);
            _spriteMesh.SetAttributes(EAttribute.Vec3, EAttribute.Vec2, EAttribute.Vec3, EAttribute.Vec3);
            var data = new float[] {
                -.95f,0,.4f,
                0,0,
                0,1,0,
                1,0,0,

                .95f,0,.4f,
                1,0,
                0,1,0,
                1,0,0,

                -.95f,2.1f,-.7f,
                0,1,
                0,1,0,
                1,0,0,

                .95f,2.1f,-.7f,
                1,1,
                0,1,0,
                1,0,0
            };
            _spriteMesh.SetData(data, new uint[] { 0, 1, 2, 2, 1, 3 });
        }


        public override void OnUpdate(FrameData data)
        {
            var sprites = World.GetComponents<CSprite>();

            foreach (var sprite in sprites)
            {
                if (sprite.Renderers.Count == 0)
                {
                    if (sprite.Textures != null)
                    {
                        var material = new Material(CustomShader.CutoffShader);
                        var renderer = new SingleRenderer
                        {
                            Material = material,
                            Mesh = _spriteMesh
                        };
                        World.GetScene(sprite.Entity.Map).Add(renderer);
                        sprite.Renderers.Add((renderer, Vec3.Zero));
                    }
                    else continue;
                }

                var textureIndex = 0;
                var orientation = sprite.Entity.Get<COrientation>();
                if (orientation != null)
                {
                    switch (orientation.Direction)
                    {
                        case COrientation.EDirection.Forward: textureIndex = sprite.ForwardIndex; break;
                        case COrientation.EDirection.Right: textureIndex = sprite.RightIndex; break;
                        case COrientation.EDirection.Backward: textureIndex = sprite.BackwardIndex; break;
                        case COrientation.EDirection.Left: textureIndex = sprite.LeftIndex; break;
                    }
                }
                if (sprite.AnimationOffsets?.Length > 0)
                {
                    if (!sprite.OnlyAnimateWhenMoving || sprite.Entity.Get<CMoveable>()?.Offset.SqrLength > 0)
                    {
                        var stateDelta = data.DeltaTime * sprite.AnimationFps / sprite.AnimationOffsets.Length;
                        sprite.AnimationState += stateDelta;
                        while (sprite.AnimationState >= 1)
                            sprite.AnimationState -= 1;
                        textureIndex += sprite.AnimationOffsets[Math.FloorToInt(sprite.AnimationState * sprite.AnimationOffsets.Length) % sprite.AnimationOffsets.Length];
                    }
                }
                sprite.Renderers[0].Renderer.Material.SetAttribute("Albedo", sprite.Textures[textureIndex % sprite.Textures.Length]);
            }

            foreach (var rendering in sprites)
                foreach (var renderer in rendering.Renderers)
                    renderer.Renderer.Position = rendering.Entity.CenterVec + renderer.Offset;
        }

        [OnRemoveComponent]
        [BeforeMapChange]
        public void RemoveRenderers(CSprite sprite) => RemoveRenderers((CRendering)sprite);
        private void RemoveRenderers(CRendering rendering)
        {
            var scene = World.GetScene(rendering.Entity.Map);
            lock (rendering.Renderers)
            {
                foreach (var v in rendering.Renderers)
                    scene.Remove(v.Renderer);
                rendering.Renderers.Clear();
            }
        }
    }
}
