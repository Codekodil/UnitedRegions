using _3DModels;
using System;
using System.Collections.Generic;
using UnhedderEngine;

namespace AssetExtractor
{
    public static class TextureLoader
    {
        public static IEnumerable<Texture> Load(string path, IEnumerable<Tuple<int, int>> texturePaletteIndices)
        {
            var btx0 = BTX0.Read(path, 0, null);

            foreach (var v in texturePaletteIndices)
            {
                var bmp = BTX0.GetTexture(null, btx0, v.Item1, v.Item2);

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
                var texture = new Texture { Interpolation = false };
                texture.ApplyNewData(bmp.Width, bmp.Height, textureSource, BaseTexture.EColorChannel.Rgba);

                yield return texture;
            }
        }
    }
}
