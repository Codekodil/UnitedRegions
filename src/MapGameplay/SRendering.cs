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
            var renderings = World.GetComponents<CRendering>();
            foreach (var rendering in renderings)
            {
                var sprite = rendering.Entity.Get<CSprite>();
                if (sprite == null)
                {
                    foreach (var renderer in rendering.Renderers)
                        World.Scene.Remove(renderer.Renderer);
                    rendering.Renderers.Clear();
                }
                else if (rendering.Renderers.Count == 0)
                {
                    var material = new Material(CustomShader.CutoffShader)
                    {
                        { "Albedo", sprite.Texture }
                    };
                    var renderer = new SingleRenderer
                    {
                        Material = material,
                        Mesh = _spriteMesh
                    };
                    World.Scene.Add(renderer);
                    rendering.Renderers.Add((renderer, Vec3.Zero));
                }

                foreach (var renderer in rendering.Renderers)
                    renderer.Renderer.Position = rendering.Entity.CenterVec + renderer.Offset;
            }
        }
    }
}
