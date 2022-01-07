using Images;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnhedderEngine;
using static UnhedderEngine.BaseTexture;

namespace AssetExtractor
{
    public static class SpriteLoader
    {
        public static Bitmap LoadBitmap(string imagePath, string palettePath, int? width, int? height, bool transparent) =>
            LoadBitmap(imagePath, palettePath, null, width, height, transparent);
        public static Bitmap LoadBitmap(string imagePath, string palettePath, string mapPath, int? width, int? height, bool transparent)
        {
            string mapHeader = null;
            if (mapPath != null)
            {
                var mapHeaderBuffer = new char[4];
                using (var file = new StreamReader(mapPath))
                    file.ReadBlock(mapHeaderBuffer, 0, mapHeaderBuffer.Length);
                mapHeader = new string(mapHeaderBuffer);
            }

            var image = new NCGR(imagePath, 0);
            var palette = new NCLR(palettePath, 0);
            Bitmap bitmap;
            switch (mapHeader)
            {
                case "RCSN":
                    var map = new NSCR(mapPath, 0);
                    if (width.HasValue) map.Width = width.Value;
                    if (height.HasValue) map.Height = height.Value;
                    bitmap = (Bitmap)map.Get_Image(image, palette);
                    break;
                case "RECN":
                    var sprite = new NCER(mapPath, 0);
                    bitmap = (Bitmap)sprite.Get_Image(image, palette, 0, width??512, height??256, false, false, false, transparent, true);
                    break;
                default:
                    if (width.HasValue) image.Width = width.Value;
                    if (height.HasValue) image.Height = height.Value;
                    bitmap = (Bitmap)image.Get_Image(palette);
                    break;
            }
            if (transparent)
                bitmap.MakeTransparent(palette.Palette[0][0]);
            return bitmap;
        }
        public static Texture LoadTexture(string imagePath, string palettePath, int? width, int? height, bool transparent) =>
            LoadTexture(imagePath, palettePath, null, width, height, transparent);
        public static Texture LoadTexture(string imagePath, string palettePath, string mapPath, int? width, int? height, bool transparent)
        {
            var bitmap = LoadBitmap(imagePath, palettePath, mapPath, width, height, transparent);
            var widthBitmap = 1;
            var heightBitmap = 1;
            var step = 1;
            byte[] pixels = null;
            widthBitmap = bitmap.Width;
            heightBitmap = bitmap.Height;

            var pixelCount = widthBitmap * heightBitmap;
            var rect = new Rectangle(0, 0, widthBitmap, heightBitmap);

            var depth = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (depth != 8 && depth != 24 && depth != 32) throw new BadImageFormatException($"image depth {depth} not supported");

            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            step = depth / 8;
            pixels = new byte[pixelCount * step];

            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            var flipBuffer = new byte[heightBitmap];
            for (var x = 0; x < widthBitmap * step; x++)
            {
                int index(int y) => x + y * widthBitmap * step;
                for (var y = 0; y < heightBitmap; y++)
                    flipBuffer[y] = pixels[index(heightBitmap - 1 - y)];
                for (var y = 0; y < heightBitmap; y++)
                    pixels[index(y)] = flipBuffer[y];
            }

            if (step == 3)
                for (var i = 0; i < pixelCount; i++)
                {
                    var temp = pixels[i * 3];
                    pixels[i * 3] = pixels[i * 3 + 2];
                    pixels[i * 3 + 2] = temp;
                }
            else if (step == 4)
            {
                for (var i = 0; i < pixelCount; i++)
                {
                    var temp = pixels[i * 4];
                    pixels[i * 4] = pixels[i * 4 + 2];
                    pixels[i * 4 + 2] = temp;
                }
            }
            bitmap.UnlockBits(data);

            var result = new Texture { Interpolation = false };
            result.ApplyNewData(widthBitmap, heightBitmap, pixels, (EColorChannel)step);
            return result;
        }
    }
}
