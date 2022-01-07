using AssetExtractor;

namespace Gen4Assets
{
    public class CommonBattleAssets : AssetManager
    {
        [Path(EGame.Platinum, "battle", "graphic", "pl_batt_obj.narc")]


        [File("pl_batt_obj_200.bin/pl_batt_obj_200.bin", "pl_batt_obj_71.RLCN", "pl_batt_obj_199.bin/pl_batt_obj_199.bin")]
        [Sprite(128, 32)] public SpriteAsset PlayerHealthDisplay { get; private set; }

        [File("pl_batt_obj_197.bin/pl_batt_obj_197.bin", "pl_batt_obj_71.RLCN", "pl_batt_obj_196.bin/pl_batt_obj_196.bin")]
        [Sprite(128, 32)] public SpriteAsset OpponentHealthDisplay { get; private set; }


        [File("pl_batt_obj_130.bin/pl_batt_obj_130.bin", "pl_batt_obj_1.RLCN", "pl_batt_obj_131.bin/pl_batt_obj_131.bin")]
        [Sprite(128, 32)] public SpriteAsset BattleGroundCircle { get; private set; }
    }
}
