using _3DModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnhedderEngine;
using static UnhedderEngine.Mesh;
using Math = UnhedderEngine.Math;

namespace AssetExtractor
{
    public static class ModelLoader
    {
        public class ModelData
        {
            public ModelData(List<(Mesh Mesh, Texture Texture)> data, Vec3 min, Vec3 max)
            {
                Data = data.AsReadOnly();
                Min = min;
                Max = max;
            }
            public IReadOnlyList<(Mesh Mesh, Texture Texture)> Data { get; }
            public Vec3 Min { get; }
            public Vec3 Max { get; }
        }

        public static ModelData LoadModel(string path)
        {
            var model = BMD0.Read(path, 0, null);
            var mdlData = model.model.mdlData[0];

            sBTX0 btx0 = new sBTX0();
            btx0.texture = model.texture;
            btx0.file = model.filePath;
            btx0.header.offset = new uint[1];
            btx0.header.offset[0] = model.header.offset[1];

            for (int c = 0; c < mdlData.bones.commands.Count; c++)
            {
                sBMD0.Model.ModelData.Bones.Command cmd = mdlData.bones.commands[c];
                switch (cmd.command)
                {
                    case 0x44:
                    case 0x24:
                    case 0x04:
                        mdlData.polygon.display[cmd.parameters[2]].materialID = cmd.parameters[0];
                        break;
                    default:
                        break;
                }
            }

            var textures = new Dictionary<Tuple<byte, byte>, Texture>();

            var min = Vec3.One * float.PositiveInfinity;
            var max = Vec3.One * float.NegativeInfinity;
            var data = mdlData.polygon.display.Select(d =>
            {
                var material = mdlData.material.material[d.materialID];
                var textureIndex = Tuple.Create(material.texID, material.palID);
                if (!textures.TryGetValue(textureIndex, out var texture))
                {
                    var bmp = BTX0.GetTexture(null, btx0, material.texID, material.palID);
                    var textureSource = new float[bmp.Width * bmp.Height * 4];
                    var textureSourceIndex = 0;
                    for (var y = bmp.Height - 1; y >= 0; --y)
                        for (var x = 0; x < bmp.Width; ++x)
                        {
                            var pixel = bmp.GetPixel(x, y);
                            textureSource[textureSourceIndex++] = pixel.R / 255f;
                            textureSource[textureSourceIndex++] = pixel.G / 255f;
                            textureSource[textureSourceIndex++] = pixel.B / 255f;
                            textureSource[textureSourceIndex++] = pixel.A / 255f;
                        }
                    textures[textureIndex] = texture = new Texture { Interpolation = false };
                    texture.ApplyNewData(bmp.Width, bmp.Height, textureSource, BaseTexture.EColorChannel.Rgba);
                }

                OpenTK.GL.StartNewModel();
                BMD0.GeometryCommands(d.commands);
                var modelData = OpenTK.GL.GetModel();

                for (var i = 4; i < modelData.Item1.Length; i += 11)
                {
                    var pos = new Vec3(modelData.Item1[i - 4], modelData.Item1[i - 3], modelData.Item1[i - 2]);
                    min = Math.Min(min, pos);
                    max = Math.Max(max, pos);
                    modelData.Item1[i - 1] /= texture.Width;
                    modelData.Item1[i] = 1 - (modelData.Item1[i] / texture.Height);
                }

                var mesh = new Mesh(false);
                mesh.SetAttributes(EAttribute.Vec3, EAttribute.Vec2, EAttribute.Vec3, EAttribute.Vec3);
                mesh.SetData(modelData.Item1, modelData.Item2);
                return (mesh, texture);

            }).ToList();

            return new ModelData(data, min, max);
        }

    }
}
