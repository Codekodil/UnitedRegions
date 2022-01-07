using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhedderEngine;

namespace AssetExtractor
{
    public class AssetManager
    {
        public enum EGame
        {
            Platinum,
            HgSs
        }
        public class PathAttribute : Attribute
        {
            public PathAttribute(EGame game, params string[] path)
            {
                var combined = System.IO.Path.Combine(path);
                switch (game)
                {
                    case EGame.Platinum:
                        combined = System.IO.Path.Combine(RomLoader.PlatinumPath, combined);
                        break;
                    case EGame.HgSs:
                        combined = System.IO.Path.Combine(RomLoader.HgSsPath, combined);
                        break;
                }
                Path = combined;
            }
            public string Path { get; }
        }
        public class FileAttribute : Attribute
        {
            public FileAttribute(params string[] names)
            {
                for (var i = 0; i < names.Length; ++i)
                    names[i] = System.IO.Path.Combine(names[i].Split('/'));
                Names = Array.AsReadOnly(names);
            }
            public IReadOnlyList<string> Names { get; }
        }

        public class SpriteAttribute : Attribute
        {
            public SpriteAttribute(int width, int height, bool transparent = true)
            {
                Width = width;
                Height = height;
                Transparent = transparent;
            }
            public int Width { get; }
            public int Height { get; }
            public bool Transparent { get; }
        }

        public abstract class BaseAsset<T> where T : class
        {
            private readonly object _locker = new object();
            public IReadOnlyList<string> Paths { get; private set; }
            public SpriteAttribute Sprite { get; private set; }
            private T _value { get; set; }
            public T Value
            {
                get
                {
                    lock (_locker)
                        return _value ?? (_value = Load());
                }
            }
            protected abstract T Load();
            public static implicit operator T(BaseAsset<T> a) => a.Value;
        }


        public AssetManager()
        {
            RomLoader.EnsureAssetsExist();
            var type = GetType();

            PathAttribute lastPath = null;
            FileAttribute lastFiles = null;

            foreach (var property in type.GetProperties())
            {
                if (property.SetMethod != null && property.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(BaseAsset<>))
                {
                    SpriteAttribute sprite = null;
                    foreach (var customAttribute in property.GetCustomAttributes(false))
                    {
                        switch (customAttribute)
                        {
                            case PathAttribute pathAttribute: lastPath = pathAttribute; break;
                            case FileAttribute fileAttribute: lastFiles = fileAttribute; break;
                            case SpriteAttribute spriteAttribute: sprite = spriteAttribute; break;
                        }
                    }

                    var instance = Activator.CreateInstance(property.PropertyType);
                    property.PropertyType.BaseType
                        .GetProperty(nameof(BaseAsset<object>.Paths))
                        .SetMethod
                        .Invoke(instance, new[] {
                            lastFiles.Names
                            .Select(n=>System.IO.Path.Combine(lastPath.Path, n))
                            .ToList().AsReadOnly()});
                    if (sprite != null)
                        property.PropertyType.BaseType
                            .GetProperty(nameof(BaseAsset<object>.Sprite))
                            .SetMethod
                            .Invoke(instance, new[] { sprite });
                    property.SetMethod.Invoke(this, new[] { instance });
                }
            }
        }



        public class ModelAsset : BaseAsset<ModelLoader.ModelData>
        {
            protected override ModelLoader.ModelData Load() => ModelLoader.LoadModel(Paths[0]);
        }

        public class SpriteAsset : BaseAsset<Texture>
        {
            protected override Texture Load() => Paths.Count <= 2 ?
                SpriteLoader.LoadTexture(Paths[0], Paths[1], Sprite?.Width, Sprite?.Height, Sprite?.Transparent ?? false) :
                SpriteLoader.LoadTexture(Paths[0], Paths[1], Paths[2], Sprite?.Width, Sprite?.Height, Sprite?.Transparent ?? false);
        }

        public class TextureIdsAttribute : Attribute
        {
            public TextureLoader.IdOrFilter TextureId { get; }
            public TextureLoader.IdOrFilter PaletteId { get; }
            public TextureIdsAttribute(int textureId, int paletteId)
            {
                TextureId = new TextureLoader.IdOrFilter() { Id = textureId };
                PaletteId = new TextureLoader.IdOrFilter() { Id = paletteId };
            }
            public TextureIdsAttribute(string textureFilter, int paletteId)
            {
                TextureId = new TextureLoader.IdOrFilter() { Filter = textureFilter };
                PaletteId = new TextureLoader.IdOrFilter() { Id = paletteId };
            }
            public TextureIdsAttribute(int textureId, string paletteFilter)
            {
                TextureId = new TextureLoader.IdOrFilter() { Id = textureId };
                PaletteId = new TextureLoader.IdOrFilter() { Filter = paletteFilter };
            }
            public TextureIdsAttribute(string textureFilter, string paletteFilter)
            {
                TextureId = new TextureLoader.IdOrFilter() { Filter = textureFilter };
                PaletteId = new TextureLoader.IdOrFilter() { Filter = paletteFilter };
            }
        }
        public class TextureAsset<T> : BaseAsset<T> where T : class
        {
            protected override T Load()
            {
                var instance = Activator.CreateInstance<T>();
                var properties = new List<PropertyInfo>();
                var ids = new List<(TextureLoader.IdOrFilter, TextureLoader.IdOrFilter)>();

                foreach (var property in typeof(T).GetProperties())
                {
                    if (property.PropertyType != typeof(Texture) || property.SetMethod == null)
                        continue;

                    var idAttribute = property.GetCustomAttributes<TextureIdsAttribute>().FirstOrDefault();
                    if (idAttribute == null)
                        continue;

                    properties.Add(property);
                    ids.Add((idAttribute.TextureId, idAttribute.PaletteId));
                }

                var textures = TextureLoader.Load(Paths[0], ids).ToList();

                for (var i = 0; i < properties.Count; ++i)
                    properties[i].SetMethod.Invoke(instance, new[] { textures[i] });

                CallAfterInit(instance);

                return instance;
            }
        }

        private static void CallAfterInit(object asset) =>
            asset.GetType().GetMethod("AfterInit")?.Invoke(asset, new object[0]);
    }
}
