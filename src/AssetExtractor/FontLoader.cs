using Fonts;
using System.Collections.Generic;
using System.Linq;
using UnhedderEngine;

namespace AssetExtractor
{
    public static class FontLoader
    {
        public static FontData LoadFont(string path)
        {
            var font = NFTR.Read(new Ekona.sFile { path = path }, null);
            var palette = NFTR.CalculatePalette(font.plgc.depth, false);
            var image = NFTR.Get_Chars(font, 512, palette);

            var charTile = new Dictionary<int, int>();
            for (int p = 0; p < font.pamc.Count; p++)
            {
                if (font.pamc[p].info is sNFTR.PAMC.Type0)
                {
                    sNFTR.PAMC.Type0 type0 = (sNFTR.PAMC.Type0)font.pamc[p].info;
                    int interval = font.pamc[p].last_char - font.pamc[p].first_char;

                    for (int j = 0; j <= interval; j++)
                        try { charTile.Add(font.pamc[p].first_char + j, type0.fist_char_code + j); }
                        catch { }
                }
                else if (font.pamc[p].info is sNFTR.PAMC.Type1)
                {
                    sNFTR.PAMC.Type1 type1 = (sNFTR.PAMC.Type1)font.pamc[p].info;

                    for (int j = 0; j < type1.char_code.Length; j++)
                    {
                        if (type1.char_code[j] == 0xFFFF)
                            continue;

                        try { charTile.Add(font.pamc[p].first_char + j, type1.char_code[j]); }
                        catch { }
                    }
                }
                else if (font.pamc[p].info is sNFTR.PAMC.Type2)
                {
                    sNFTR.PAMC.Type2 type2 = (sNFTR.PAMC.Type2)font.pamc[p].info;

                    for (int j = 0; j < type2.num_chars; j++)
                    {
                        if (type2.charInfo[j].chars == 0xFFFF)
                            continue;
                        try { charTile.Add(type2.charInfo[j].chars_code, type2.charInfo[j].chars); }
                        catch { }
                    }
                }
            }

            var charWidth = font.fnif.width;
            var pixelWidth = 1f / image.Width;
            var charHeight = font.fnif.height;
            var pixelHeight = 1f / image.Height;
            var lineWidth = image.Width / (charWidth + 2);

            var symbolData = charTile.ToDictionary(kv => (char)kv.Key, kv =>
            {
                var charData = font.hdwc.info[kv.Value];
                var xmin = 2 + (kv.Value % lineWidth) * (charWidth + 2);
                var xmax = xmin + charData.pixel_width;
                var ymax = image.Height - 2 - (kv.Value / lineWidth) * (charHeight + 2);
                var ymin = ymax - charHeight;
                return (
                    (new Vec2(xmin, ymin) + new Vec2(.01f)).Scaled(pixelWidth, pixelHeight),
                    (new Vec2(xmax, ymax) - new Vec2(.01f)).Scaled(pixelWidth, pixelHeight),
                    charData.pixel_length / (float)charHeight,
                    charData.pixel_start / (float)charHeight,
                    charData.pixel_width / (float)charHeight);
            });

            var texture = Texture.Open(image);
            texture.Interpolation = false;
            return new FontData(texture, symbolData);
        }

    }
}
