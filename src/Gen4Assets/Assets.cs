namespace Gen4Assets
{
    public class Assets
    {
        public CommonMapAssets CommonMapAssets { get; } = new CommonMapAssets();
        public CommonBattleAssets CommonBattleAssets { get; } = new CommonBattleAssets();
        public CommonGuiAssets CommonGuiAssets { get; } = new CommonGuiAssets();
        public BattleBackgroundAssets BattleBackgroundAssets { get; private set; } = new BattleBackgroundAssets();
    }
}
