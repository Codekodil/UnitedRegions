using AssetExtractor;

namespace Gen4Assets
{
    public class BattleBackgroundAssets : AssetManager
    {
        [Path(EGame.Platinum, "battle", "graphic", "pl_batt_bg.narc")]


        [File("pl_batt_bg_3.bin/pl_batt_bg_3.bin", "pl_batt_bg_172.RLCN", "pl_batt_bg_2.bin/pl_batt_bg_2.bin")]
        [Sprite(256, 160, Transparent = false)] public SpriteAsset ForestDay { get; private set; }
    }
}
