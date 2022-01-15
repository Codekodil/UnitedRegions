using AssetExtractor;
using UnhedderEngine;

namespace MonsterData
{
    public class Monster
    {
        public MonsterSpecies Species { get; }

        public string Nickname { get; set; }
        public string DisplayName => Nickname ?? Species.Name;

        public int _level;
        public int Level => _level;

        public int CurrentHP { get; set; }

        public Stats Stats { get; private set; }
        public Stats Ev { get; }
        public Stats Iv { get; }

        public Monster(MonsterSpecies species, int level)
        {
            Species = species;
            _level = level;
            UpdateStats();
            CurrentHP = Stats.HP;
        }

        public (BattleSprite Front, BattleSprite Back) LoadBattleTexture() => Species.LoadBattleTexture(false, false);

        private void UpdateStats()
        {
            var newStats = Stats;
            newStats.HP = Math.FloorToInt((2 * Species.BaseStats.HP + Iv.HP + Math.Floor(Ev.HP * .25f)) * Level * .01f) + Level + 10;
            newStats.Speed = Math.FloorToInt((2 * Species.BaseStats.Speed + Iv.Speed + Math.Floor(Ev.Speed * .25f)) * Level * .01f) + 5;//*Nature
            newStats.Attack = Math.FloorToInt((2 * Species.BaseStats.Attack + Iv.Attack + Math.Floor(Ev.Attack * .25f)) * Level * .01f) + 5;//*Nature
            newStats.Defence = Math.FloorToInt((2 * Species.BaseStats.Defence + Iv.Defence + Math.Floor(Ev.Defence * .25f)) * Level * .01f) + 5;//*Nature
            newStats.SpAttack = Math.FloorToInt((2 * Species.BaseStats.SpAttack + Iv.SpAttack + Math.Floor(Ev.SpAttack * .25f)) * Level * .01f) + 5;//*Nature
            newStats.SpDefence = Math.FloorToInt((2 * Species.BaseStats.SpDefence + Iv.SpDefence + Math.Floor(Ev.SpDefence * .25f)) * Level * .01f) + 5;//*Nature
            Stats = newStats;
        }
    }
}
