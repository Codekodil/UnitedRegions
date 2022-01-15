using AssetExtractor;
using System.Drawing;
using System.IO;

namespace Gen4Assets
{
    public class CommonGuiAssets : AssetManager
    {
        [Path(EGame.Platinum, "data", "utility.bin", "msg")]
        [File("kc_m.NFTR.l/kc_m.NFTR.l")]
        public FontAsset BaseFont { get; private set; }
        [File("lc_s.NFTR.l/lc_s.NFTR.l")]
        public FontAsset SmallFont { get; private set; }



        [Path(EGame.Platinum, "graphic", "pl_pst_gra.narc")]

        [File("pl_pst_gra_0.RGCN", "pl_pst_gra_1.RLCN"), Sprite(256, 120, 64, 50, 8, 4, PaletteIndex = 10)]
        public SpriteAsset HealthGreen { get; private set; }

        [File("pl_pst_gra_0.RGCN", "pl_pst_gra_1.RLCN"), Sprite(256, 120, 64, 58, 8, 4, PaletteIndex = 10)]
        public SpriteAsset HealthOrange { get; private set; }

        [File("pl_pst_gra_0.RGCN", "pl_pst_gra_1.RLCN"), Sprite(256, 120, 64, 66, 8, 4, PaletteIndex = 10)]
        public SpriteAsset HealthRed { get; private set; }


        [File("pl_pst_gra_0.RGCN", "../../battle/graphic/pl_batt_obj.narc/pl_batt_obj_71.RLCN"), Sprite(256, 120, 0, 49, 8, 6, PaletteIndex = 10)]
        public SpriteAsset HealthEmpty { get; private set; }





        [Path(EGame.Platinum, "graphic", "pl_winframe.narc")]
        [File("pl_winframe_3.RGCN", "pl_winframe_26.RLCN")]

        [Sprite(48, 24, 0, 0, 13, 7)] public SpriteAsset UiFrameA_TL { get; private set; }
        [Sprite(48, 24, 0, 7, 13, 10)] public SpriteAsset UiFrameA_L { get; private set; }
        [Sprite(48, 24, 0, 17, 13, 7)] public SpriteAsset UiFrameA_BL { get; private set; }

        [Sprite(48, 24, 13, 0, 12, 7)] public SpriteAsset UiFrameA_T { get; private set; }
        [Sprite(48, 24, 13, 7, 12, 10)] public SpriteAsset UiFrameA_C { get; private set; }
        [Sprite(48, 24, 13, 17, 12, 7)] public SpriteAsset UiFrameA_B { get; private set; }

        [Sprite(48, 24, 25, 0, 23, 7)] public SpriteAsset UiFrameA_TR { get; private set; }
        [Sprite(48, 24, 25, 7, 23, 10)] public SpriteAsset UiFrameA_R { get; private set; }
        [Sprite(48, 24, 25, 17, 23, 7)] public SpriteAsset UiFrameA_BR { get; private set; }

    }
}
