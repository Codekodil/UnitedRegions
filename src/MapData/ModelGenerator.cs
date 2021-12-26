using AssetExtractor;
using Gen4Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnhedderEngine;
using UnhedderEngine.Workflows.Core;
using static UnhedderEngine.Mesh;

namespace MapData
{
    public class ModelGenerator
    {
        private static readonly EGround MaxGround;
        static ModelGenerator()
        {
            MaxGround = Enum.GetValues(typeof(EGround)).Cast<EGround>().Max();
        }

        public CommonMapAssets CommonMapAssets { get; set; }

        public List<BaseRenderer> GenerateRenderers(MapChunk chunk)
        {
            if (CommonMapAssets == null)
                CommonMapAssets = new CommonMapAssets();

            var tiledData = new Dictionary<object, Tuple<List<float>, List<uint>, Texture>>();

            for (var x = 0; x < MapChunk.ChunkSize; ++x)
                for (var y = 0; y < MapChunk.ChunkSize; ++y)
                {
                    var ground = chunk.GetGround(x, y);
                    var edge = chunk.GetEdge(x, y);
                    var height = chunk.GetHeight(x, y);

                    AddTilesData(edge == 0 ? ground : ground + (int)MaxGround + 1, x, y, height, edge);
                    var tile = chunk.GetTile(x, y);
                    switch (tile)
                    {
                        case ETile.SmallCliffs:
                            AddTilesData(tile, x, y, height, chunk.GetData(x, y) is EEdge e ? e : default);
                            break;
                        case ETile.TallGrass:
                            AddTilesData(tile, x, y, height, 0);
                            break;
                    }
                }

            void AddTilesData(object key, int x, int y, int height, EEdge edge)
            {
                if (!tiledData.TryGetValue(key, out var data))
                {
                    Texture texture = null;
                    switch (key)
                    {
                        case EGround groundKey:
                            switch (groundKey > MaxGround ? (EGround)(groundKey - MaxGround - 1) : groundKey)
                            {
                                case EGround.Sand:
                                    texture = edge == 0 ?
                                        CommonMapAssets.CommonTiles.Value.Sand :
                                        CommonMapAssets.CommonTiles.Value.SandEdge; break;
                            }
                            break;
                        case ETile tileKey:
                            switch (tileKey)
                            {
                                case ETile.SmallCliffs:
                                    texture = CommonMapAssets.CommonTiles.Value.SmallCliffs; break;
                                case ETile.TallGrass:
                                    texture = CommonMapAssets.TallGrass.Value.Frame1; break;
                            }
                            break;
                    }
                    if (texture == null)
                        texture = CommonMapAssets.CommonTiles.Value.Grass;
                    tiledData[key] = data = Tuple.Create(new List<float>(), new List<uint>(), texture);

                }
                var heightOffset = key is ETile ? .01f : 0;

                switch (edge)
                {
                    case EEdge.Top: AddData(.5f, .5f, -.5f, 0, 0, -.5f, -Vec3.OneX); break;
                    case EEdge.Right: AddData(0, .5f, 0, -.5f, .5f, 0, Vec3.OneZ); break;
                    case EEdge.Bottom: AddData(0, 0, .5f, 0, 0, .5f, Vec3.OneX); break;
                    case EEdge.Left: AddData(.5f, 0, 0, .5f, -.5f, 0, -Vec3.OneZ); break;

                    case EEdge.CornerTopRight: AddData(.5f, .5f, 0, .5f, -.5f, 0, -Vec3.OneZ); break;
                    case EEdge.CornerBottomRight: AddData(.5f, 1, -.5f, 0, 0, -.5f, -Vec3.OneX); break;
                    case EEdge.CornerBottomLeft: AddData(0, 1, 0, -.5f, .5f, 0, Vec3.OneZ); break;
                    case EEdge.CornerTopLeft: AddData(0, .5f, .5f, 0, 0, .5f, Vec3.OneX); break;

                    case EEdge.InnerCornerTopRight: AddData(1, .5f, 0, .5f, -.5f, 0, -Vec3.OneZ); break;
                    case EEdge.InnerCornerBottomRight: AddData(1, 1, -.5f, 0, 0, -.5f, -Vec3.OneX); break;
                    case EEdge.InnerCornerBottomLeft: AddData(.5f, 1, 0, -.5f, .5f, 0, Vec3.OneZ); break;
                    case EEdge.InnerCornerTopLeft: AddData(.5f, .5f, .5f, 0, 0, .5f, Vec3.OneX); break;

                    case EEdge.TopCutLeft: AddData(1, .5f, -.5f, 0, 0, -.5f, -Vec3.OneX); break;
                    case EEdge.RightCutTop: AddData(.5f, .5f, 0, -.5f, .5f, 0, Vec3.OneZ); break;
                    case EEdge.BottomCutRight: AddData(.5f, 0, .5f, 0, 0, .5f, Vec3.OneX); break;
                    case EEdge.LeftCutBottom: AddData(1, 0, 0, .5f, -.5f, 0, -Vec3.OneZ); break;

                    case EEdge.TopCutRight: AddData(.5f, .5f, .5f, 0, 0, -.5f, -Vec3.OneX); break;
                    case EEdge.RightCutBottom: AddData(1, .5f, 0, -.5f, -.5f, 0, Vec3.OneZ); break;
                    case EEdge.BottomCutLeft: AddData(1, 0, -.5f, 0, 0, .5f, Vec3.OneX); break;
                    case EEdge.LeftCutTop: AddData(.5f, 0, 0, .5f, .5f, 0, -Vec3.OneZ); break;

                    default: AddData(0, 0, 1, 0, 0, 1, Vec3.OneX); break;
                }
                void AddData(float u, float v, float ux, float vx, float uy, float vy, Vec3 tangent)
                {
                    var index = data.Item1.Count / 11;
                    data.Item1.AddRange(new[] { x, height + heightOffset, -y, u, v, 0, 1, 0, tangent.X, tangent.Y, tangent.Z });
                    data.Item1.AddRange(new[] { x + 1, height + heightOffset, -y, u + ux, v + vx, 0, 1, 0, tangent.X, tangent.Y, tangent.Z });
                    data.Item1.AddRange(new[] { x + 1, height + heightOffset, -y - 1, u + ux + uy, v + vx + vy, 0, 1, 0, tangent.X, tangent.Y, tangent.Z });
                    data.Item1.AddRange(new[] { x, height + heightOffset, -y - 1, u + uy, v + vy, 0, 1, 0, tangent.X, tangent.Y, tangent.Z });
                    data.Item2.AddRange(new[] { 0, 1, 2, 0, 2, 3 }.Select(i => (uint)(i + index)));
                }
            }



            var result = new List<BaseRenderer>();
            Vec3 chunkPositionOffset = new Vec3(chunk.MapXY.X, 0, -chunk.MapXY.Y) * MapChunk.ChunkSize;

            var instancedRenderers = new Dictionary<ETile, List<(InstancedRenderer Renderer, List<Mat4> Transforms)>>();

#if DEBUG
            var undefinedModelData = new ModelLoader.ModelData(new List<(Mesh Mesh, Texture Texture)> { (Cube, Texture.SolidColor("F0F")) }, Vec3.Zero, Vec3.Zero);
            var outOfBondModelData = new ModelLoader.ModelData(new List<(Mesh Mesh, Texture Texture)> { (Cube, Texture.SolidColor("050")) }, Vec3.Zero, Vec3.Zero);
#endif

            for (var x = 0; x < MapChunk.ChunkSize; ++x)
                for (var y = 0; y < MapChunk.ChunkSize; ++y)
                {
                    ModelLoader.ModelData model = null;
                    var scale = Vec3.One;
                    var offset = Vec3.Zero;
                    var tile = chunk.GetTile(x, y);
                    var instanced = false;
                    switch (tile)
                    {
#if DEBUG
                        case ETile.Undefined:
                            model = undefinedModelData;
                            offset = new Vec3(.5f, 0, -.5f);
                            scale *= .5f;
                            instanced = true;
                            break;
                        case ETile.OutOfBound:
                            model = outOfBondModelData;
                            offset = new Vec3(.5f, 1, -.5f);
                            scale *= .5f;
                            instanced = true;
                            break;
#endif
                        case ETile.Tree:
                            model = CommonMapAssets.Tree;
                            instanced = true;
                            break;
                        case ETile.SmallBush:
                            model = CommonMapAssets.SmallBush;
                            instanced = true;
                            break;
                        case ETile.HealCenter:
                            model = CommonMapAssets.HealCenter;
                            scale *= .5f;
                            offset = new Vec3(.5f, 0, -2);
                            break;
                        case ETile.Shop:
                            model = CommonMapAssets.Shop;
                            scale *= .5f;
                            offset = new Vec3(1, 0, -2);
                            break;
                        case ETile.House:
                            model = CommonMapAssets.HouseSmall1;
                            offset = new Vec3(1, 0, -2);
                            break;
                    }
                    if (model != null)
                    {
                        if (instanced)
                        {
                            if (!instancedRenderers.TryGetValue(tile, out var renderers))
                            {
                                instancedRenderers[tile] = renderers = model.Data
                                    .Select(d =>
                                    {
                                        var centerMaterial = new Material(CustomShader.CutoffShader)
                                            { { "Albedo", d.Texture} };
                                        return (new InstancedRenderer
                                        {
                                            Mesh = d.Item1,
                                            Material = centerMaterial
                                        }, new List<Mat4>());
                                    })
                                    .ToList();
                                foreach (var renderer in renderers)
                                    result.Add(renderer.Renderer);
                            }
                            foreach (var renderer in renderers)
                                renderer.Transforms.Add(Mat4.FromPositionRotationScale(new Vec3(x, 0, -y) + offset + chunkPositionOffset, Quaternion.NoRotation, scale));
                        }
                        else
                            foreach (var v in model.Data)
                            {
                                var centerMaterial = new Material(CustomShader.CutoffShader)
                            { { "Albedo", v.Texture} };
                                result.Add(new SingleRenderer
                                {
                                    Mesh = v.Mesh,
                                    Material = centerMaterial,
                                    Position = new Vec3(x, 0, -y) + offset,
                                    Scale = scale
                                });
                            }
                    }
                }

            foreach (var renderer in instancedRenderers.SelectMany(kv => kv.Value))
            {
                renderer.Renderer.SetAttributes(EAttribute.Mat4);
                renderer.Renderer.SetData(renderer.Transforms.SelectMany(m => m).ToArray());
            }

            foreach (var data in tiledData.Values)
            {
                var groundMesh = new Mesh(false);
                groundMesh.SetAttributes(EAttribute.Vec3, EAttribute.Vec2, EAttribute.Vec3, EAttribute.Vec3);
                groundMesh.SetData(data.Item1.ToArray(), data.Item2.ToArray());

                var material = new Material(CustomShader.CutoffShader)
                { { "Albedo", data.Item3} };

                result.Add(new SingleRenderer
                {
                    Mesh = groundMesh,
                    Material = material
                });
            }

            foreach (var renderer in result.OfType<SingleRenderer>())
                renderer.Position += chunkPositionOffset;
            return result;
        }
    }
}
