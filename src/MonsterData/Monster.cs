using AssetExtractor;

namespace MonsterData
{
    public class Monster
    {
        public MonsterSpecies Species { get; }

        public int Level { get; }

        public int CurrentHP { get; set; }

        public Stats Stats { get; }

        public Monster(MonsterSpecies species, int level)
        {
            Species = species;
            Level = level;
            Stats = Species.BaseStats;
            CurrentHP = Stats.HP;
        }

        public (BattleSprite Front, BattleSprite Back) LoadBattleTexture() => Species.LoadBattleTexture(false, false);

    }
}
