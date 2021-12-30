using AssetExtractor;
using UnhedderEngine;

namespace Gen4Assets
{
    public class CommonMapAssets : AssetManager
    {
        public CommonMapAssets()
        {
            Tree = new CustomModelAsset(
                () => MeshParserObj.Open(typeof(CommonMapAssets).Assembly.GetManifestResourceStream("Gen4Assets.CustomAssets.Tree.obj")).Objects[0].Groups[0].ToMesh(),
                () => CommonTiles.Value.Tree);
            SmallBush = new CustomModelAsset(
                () => MeshParserObj.Open(typeof(CommonMapAssets).Assembly.GetManifestResourceStream("Gen4Assets.CustomAssets.SmallBush.obj")).Objects[0].Groups[0].ToMesh(),
                () => CommonTiles.Value.SmallObstructions);
        }



        [Path(EGame.Platinum, "fielddata", "build_model", "build_model.narc")]

        [File("build_model_4.BMD0")] public ModelAsset HealCenter { get; private set; }
        [File("build_model_5.BMD0")] public ModelAsset Shop { get; private set; }
        [File("build_model_22.BMD0")] public ModelAsset HouseSmall1 { get; private set; }
        public CustomModelAsset Tree { get; }
        public CustomModelAsset SmallBush { get; }



        [Path(EGame.Platinum, "fielddata", "areadata", "area_map_tex", "map_tex_set.narc")]

        [File("map_tex_set_9.BTX0")] public TextureAsset<CommonTilesClass> CommonTiles { get; private set; }
        public class CommonTilesClass
        {
            [TextureIds(0, 0)] public Texture SmallCliffs { get; private set; }
            [TextureIds(33, 20)] public Texture Grass { get; private set; }
            [TextureIds(35, 41)] public Texture Sand { get; private set; }
            [TextureIds(36, 33)] public Texture SandEdge { get; private set; }

            [TextureIds(24, 24)] public Texture SmallObstructions { get; private set; }

            [TextureIds(50, 48)] public Texture Tree { get; private set; }
        }

        [Path(EGame.Platinum, "data", "mmodel", "fldeff.narc")]

        [File("fldeff_1.BTX0")] public TextureAsset<TallGrassClass> TallGrass { get; private set; }
        public class TallGrassClass
        {
            [TextureIds(0, 0)] public Texture Frame1 { get; private set; }
            [TextureIds(1, 0)] public Texture Frame2 { get; private set; }
            [TextureIds(2, 0)] public Texture Frame3 { get; private set; }
        }



        [Path(EGame.Platinum, "data", "mmodel", "mmodel.narc")]
        [File("mmodel_91.BTX0")] public TextureAsset<NpcTextures> FemalePlayer { get; private set; }
        public class NpcTextures
        {
            public void AfterInit()
            {
                Frames = new Texture[] {
                    Forward, ForwardWalk1, ForwardWalk2,
                    Right, RightWalk1, RightWalk2,
                    Backward, BackwardWalk1, BackwardWalk2,
                    Left, LeftWalk1, LeftWalk2
                };
            }
            public Texture[] Frames { get; private set; }
            [TextureIds(".*\\.1", 0)] public Texture Forward { get; private set; }
            [TextureIds(".*\\.2", 0)] public Texture ForwardWalk1 { get; private set; }
            [TextureIds(".*\\.4", 0)] public Texture ForwardWalk2 { get; private set; }

            [TextureIds(".*\\.13", 0)] public Texture Right { get; private set; }
            [TextureIds(".*\\.14", 0)] public Texture RightWalk1 { get; private set; }
            [TextureIds(".*\\.16", 0)] public Texture RightWalk2 { get; private set; }

            [TextureIds(".*\\.5", 0)] public Texture Backward { get; private set; }
            [TextureIds(".*\\.6", 0)] public Texture BackwardWalk1 { get; private set; }
            [TextureIds(".*\\.8", 0)] public Texture BackwardWalk2 { get; private set; }

            [TextureIds(".*\\.9", 0)] public Texture Left { get; private set; }
            [TextureIds(".*\\.10", 0)] public Texture LeftWalk1 { get; private set; }
            [TextureIds(".*\\.12", 0)] public Texture LeftWalk2 { get; private set; }
        }
    }
}
