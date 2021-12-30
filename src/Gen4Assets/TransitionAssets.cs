using AssetExtractor;

namespace Gen4Assets
{
    public class TransitionAssets : AssetManager
    {

        [Path(EGame.Platinum, "graphic", "ending.narc")]

        [File("ending_82.BMD0")] public ModelAsset Tree { get; private set; }

    }
}
