using AssetExtractor;
using System;
using System.Collections.Generic;
using UnhedderEngine;
using static AssetExtractor.AssetManager;

namespace Gen4Assets
{
    public class CustomModelAsset : BaseAsset<ModelLoader.ModelData>
    {
        private readonly Func<Mesh> _getMesh;
        private readonly Func<Texture> _getTexture;
        public CustomModelAsset(Func<Mesh> getMesh, Func<Texture> getTexture)
        {
            _getMesh = getMesh;
            _getTexture = getTexture;
        }

        protected override ModelLoader.ModelData Load() =>
            new ModelLoader.ModelData(new List<(Mesh, Texture)> { (_getMesh(), _getTexture()) }, Vec3.Zero, Vec3.Zero);
    }
}
