using AssetExtractor;
using System;
using UnhedderEngine;

namespace MonsterData
{
    public class MonsterSpecies
    {
        public int Id { get; }
        public string Name { get; }
        public Stats BaseStats { get; }

        public DamageType Type1 { get; }
        public DamageType Type2 { get; }

        public int CatchRate { get; }
        public int ExpYield { get; }
        public Stats EvYield { get; }

        public float? GenderChance { get; }
        public int HatchingSteps { get; }
        public int BaseFriendship { get; }
        public EExpType ExpType { get; }
        public int EggGroup1 { get; }
        public int EggGroup2 { get; }


        internal MonsterSpecies(
            int id,
            string name,
            Stats baseStats,
            DamageType type1,
            DamageType type2,
            int catchRate,
            int expYield,
            Stats evYield,
            float? genderChance,
            int hatchingSteps,
            int baseFriendship,
            EExpType expType,
            int eggGroup1,
            int eggGroup2)
        {
            Id = id;
            Name = name;
            BaseStats = baseStats;
            Type1 = type1;
            Type2 = type2;
            CatchRate = catchRate;
            ExpYield = expYield;
            EvYield = evYield;
            GenderChance = genderChance;
            HatchingSteps = hatchingSteps;
            BaseFriendship = baseFriendship;
            ExpType = expType;
            EggGroup1 = eggGroup1;
            EggGroup2 = eggGroup2;
        }

        public (BattleSprite Front, BattleSprite Back) LoadBattleTexture(bool female, bool shiny)
        {
            try
            {
                return (BattleSpriteLoader.LoadSprite(Id, false, female, shiny),
                    BattleSpriteLoader.LoadSprite(Id, true, female, shiny));
            }
            catch(Exception)
            {
                return (BattleSpriteLoader.LoadSprite(Id, false, !female, shiny),
                    BattleSpriteLoader.LoadSprite(Id, true, !female, shiny));
            }
        }

        public override string ToString() => Name.Substring(0, 1) + Name.Substring(1).ToLower() + " " + Type1.ToString() + (Type1 != Type2 ? Type2.ToString() : "");
    }
}
