using _3DModels;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnhedderEngine;

namespace AssetExtractor
{
    public static class TextureLoader
    {
        public class IdOrFilter
        {
            public int Id { get; set; } = -1;
            public string Filter { get; set; }
        }

        public static IEnumerable<Texture> Load(string path, IEnumerable<(IdOrFilter Texture, IdOrFilter Palette)> texturePaletteIndices)
        {
            var btx0 = BTX0.Read(path, 0, null);

            foreach (var v in texturePaletteIndices)
            {
                var textureId = v.Texture.Id;
                if (textureId < 0)
                {
                    textureId = 0;
                    for (var i = 0; i < btx0.texture.texInfo.names.Length; ++i)
                        if (Regex.IsMatch(btx0.texture.texInfo.names[i], v.Texture.Filter))
                        {
                            textureId = i;
                            break;
                        }
                }
                var paletteId = v.Palette.Id;
                if (paletteId < 0)
                {
                    paletteId = 0;
                    for (var i = 0; i < btx0.texture.palInfo.names.Length; ++i)
                        if (Regex.IsMatch(btx0.texture.palInfo.names[i], v.Texture.Filter))
                        {
                            paletteId = i;
                            break;
                        }
                }

                var bmp = BTX0.GetTexture(null, btx0, textureId, paletteId);

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
