using Images;
using System.Drawing;
using System.IO;
using UnhedderEngine;

namespace AssetExtractor
{
    public static class SpriteLoader
    {
        public static Bitmap LoadBitmap(string imagePath, string palettePath, int paletteIndex, int? width, int? height, bool transparent, Rectangle? crop = null) =>
            LoadBitmap(imagePath, palettePath, paletteIndex, null, width, height, transparent, crop);
        public static Bitmap LoadBitmap(string imagePath, string palettePath, string mapPath, int? width, int? height, bool transparent, Rectangle? crop = null) =>
            LoadBitmap(imagePath, palettePath, 0, mapPath, width, height, transparent, crop);
        private static Bitmap LoadBitmap(string imagePath, string palettePath, int paletteIndex, string mapPath, int? width, int? height, bool transparent, Rectangle? crop)
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

            paletteIndex = palette.NumberOfPalettes > 1 ? paletteIndex % palette.NumberOfPalettes : 0;
            for (int j = 0; j < image.TilesPalette.Length; j++)
                image.TilesPalette[j] = (byte)paletteIndex;

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
                    bitmap = (Bitmap)sprite.Get_Image(image, palette, 0, width ?? 512, height ?? 256, false, false, false, transparent, true);
                    break;
                default:
                    if (width.HasValue) image.Width = width.Value;
                    if (height.HasValue) image.Height = height.Value;
                    bitmap = (Bitmap)image.Get_Image(palette);
                    break;
            }
            if (transparent)
                bitmap.MakeTransparent(palette.Palette[paletteIndex][0]);
            return crop.HasValue ? Crop(bitmap, crop.Value) : bitmap;
        }

        private static Bitmap Crop(Bitmap bitmap, Rectangle rectangle)
        {
            var cropped = new Bitmap(rectangle.Width, rectangle.Height);
            for (var x = 0; x < cropped.Width; ++x)
                for (var y = 0; y < cropped.Height; ++y)
                    cropped.SetPixel(x, y, bitmap.GetPixel(x + rectangle.X, y + rectangle.Y));
            return cropped;
        }

        public static Texture LoadTexture(string imagePath, string palettePath, int paletteIndex, int? width, int? height, bool transparent, Rectangle? crop = null) =>
            LoadTexture(imagePath, palettePath, paletteIndex, null, width, height, transparent, crop);
        public static Texture LoadTexture(string imagePath, string palettePath, string mapPath, int? width, int? height, bool transparent, Rectangle? crop = null) =>
            LoadTexture(imagePath, palettePath, 0, mapPath, width, height, transparent, crop);
        private static Texture LoadTexture(string imagePath, string palettePath, int paletteIndex, string mapPath, int? width, int? height, bool transparent, Rectangle? crop)
        {
            var texture = Texture.Open(LoadBitmap(imagePath, palettePath, paletteIndex, mapPath, width, height, transparent, crop));
            texture.Interpolation = false;
            return texture;
        }
    }
}
