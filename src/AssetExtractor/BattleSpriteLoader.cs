using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnhedderEngine;

namespace AssetExtractor
{
    public static class BattleSpriteLoader
    {
        public static Texture LoadSprite(int id)
        {
            RomLoader.EnsureAssetsExist();
            using (var spriteStream = File.OpenRead(Path.Combine(RomLoader.PlatinumPath, "poketool", "pokegra", "pl_pokegra.narc", $"pl_pokegra_{id + 2}.RGCN")))
            {
                using (var paletteStream = File.OpenRead(Path.Combine(RomLoader.PlatinumPath, "poketool", "pokegra", "pl_pokegra.narc", $"pl_pokegra_{id + 4}.RLCN")))
                {
                    return LoadSprite(spriteStream, paletteStream);
                }
            }
        }
        public static Texture LoadSprite(Stream spriteStream, Stream paletteStream)
        {
            spriteStream.Seek(48L, SeekOrigin.Current);
            BinaryReader binaryReader = new BinaryReader(spriteStream);
            var numArray1 = new ushort[3200];
            for (int i = 0; i < 3200; ++i)
                numArray1[i] = binaryReader.ReadUInt16();
            var num1 = (uint)numArray1[0];
            for (int i = 0; i < 3200; ++i)
            {
                ushort[] numArray2;
                IntPtr num2;
                (numArray2 = numArray1)[(int)(num2 = (IntPtr)i)] = (ushort)((uint)numArray2[(int)num2] ^ (uint)(ushort)(num1 & (uint)ushort.MaxValue));
                num1 = num1 * 1103515245U + 24691U;
            }
            var bitmapBuffer = new Bitmap(160, 80, PixelFormat.Format8bppIndexed);
            var rect = new Rectangle(0, 0, 160, 80);
            var source = new byte[12800];
            for (int i = 0; i < 3200; ++i)
            {
                source[i * 4] = (byte)(numArray1[i] & 15U);
                source[i * 4 + 1] = (byte)(numArray1[i] >> 4 & 15);
                source[i * 4 + 2] = (byte)(numArray1[i] >> 8 & 15);
                source[i * 4 + 3] = (byte)(numArray1[i] >> 12 & 15);
            }
            var bitmapdata = bitmapBuffer.LockBits(rect, ImageLockMode.WriteOnly, bitmapBuffer.PixelFormat);
            var scan0 = bitmapdata.Scan0;
            Marshal.Copy(source, 0, scan0, 12800);
            bitmapBuffer.UnlockBits(bitmapdata);


            paletteStream.Seek(40L, SeekOrigin.Current);
            var numArray = new ushort[16];
            binaryReader = new BinaryReader(paletteStream);
            for (int i = 0; i < 16; ++i)
                numArray[i] = binaryReader.ReadUInt16();
            var palette = new Bitmap(1, 1, PixelFormat.Format4bppIndexed).Palette;
            for (int i = 0; i < 16; ++i)
                palette.Entries[i] = Color.FromArgb((numArray[i] & 31) << 3, (numArray[i] >> 5 & 31) << 3, (numArray[i] >> 10 & 31) << 3);
            bitmapBuffer.Palette = palette;

            var textureSource = new float[bitmapBuffer.Width * bitmapBuffer.Height * 4];
            var textureSourceIndex = 0;
            for (var y = bitmapBuffer.Height - 1; y >= 0; --y)
                for (var x = 0; x < bitmapBuffer.Width; ++x)
                {
                    var pixel = bitmapBuffer.GetPixel(x, y);
                    textureSource[textureSourceIndex++] = pixel.R / 255f;
                    textureSource[textureSourceIndex++] = pixel.G / 255f;
                    textureSource[textureSourceIndex++] = pixel.B / 255f;
                    textureSource[textureSourceIndex++] = source[x + y * bitmapBuffer.Width] == 0 ? 0 : 1;
                }


            var texture = new Texture { Interpolation = false };
            texture.ApplyNewData(bitmapBuffer.Width, bitmapBuffer.Height, textureSource, BaseTexture.EColorChannel.Rgba);
            return texture;
        }
    }
}
