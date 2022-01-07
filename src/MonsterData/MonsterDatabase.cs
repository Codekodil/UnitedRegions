using AssetExtractor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MonsterData
{
    public class MonsterDatabase
    {
        public IReadOnlyList<MonsterSpecies> Monsters { get; }
        public IReadOnlyList<DamageType> Types { get; }
        public IReadOnlyDictionary<(DamageType Attacker, DamageType Defender), float> DamageMultipliers { get; }

        public MonsterDatabase()
        {
            var abilities2 = new object[724]
                .Select((_, i) => (i, string.Join(" ", MessageLoader.LoadPlatinumMessage(i))))
                .Where(m => m.Item2.ToLower().Contains("water3"))
                .ToList();

            var abilities = MessageLoader.LoadPlatinumMessage(611);
            var items = MessageLoader.LoadPlatinumMessage(392);
            var attack = MessageLoader.LoadPlatinumMessage(648);
            Types = MessageLoader.LoadPlatinumMessage(624).Select((t, i) => new DamageType(i, t)).ToList().AsReadOnly();

            var _monsters = new List<MonsterSpecies>();
            Monsters = _monsters.AsReadOnly();

            var names = MessageLoader.LoadPlatinumMessage(412);

            var _damageMultipliers = new Dictionary<(DamageType, DamageType), float>();
            DamageMultipliers = new ReadOnlyDictionary<(DamageType, DamageType), float>(_damageMultipliers);

            using (var file = File.OpenRead(Path.Combine(RomLoader.PlatinumPath, "ftc", "overlay9_16")))
            {
                file.Seek(0x33b94, SeekOrigin.Begin);
                var reader = new BinaryReader(file);
                while (true)
                {
                    var block = reader.ReadBytes(3);
                    if (block[0] >= Types.Count || block[1] >= Types.Count)
                        break;
                    _damageMultipliers[(Types[block[0]], Types[block[1]])] = block[2] / 10f;
                }
            }

            var statPath = Path.Combine(RomLoader.PlatinumPath, "poketool", "personal", "pl_personal.narc");
            var statFilenames = Directory.GetFiles(statPath);

            for (var i = 1; i < names.Count - 2; ++i)
            {
                var baseStats = new Stats();
                var evYield = new Stats();

                DamageType type1, type2;
                int catchRate, expYield, hatchingSteps, baseFriendship, eggGroup1, eggGroup2;
                float? genderChance = null;
                EExpType expType;

                var statFilePath = statFilenames.First(f => f.Contains($"pl_personal_{i}."));
                using (var statFile = new BinaryReader(File.OpenRead(statFilePath)))
                {
                    baseStats.HP = statFile.ReadByte();
                    baseStats.Attack = statFile.ReadByte();
                    baseStats.Defence = statFile.ReadByte();
                    baseStats.Speed = statFile.ReadByte();
                    baseStats.SpAttack = statFile.ReadByte();
                    baseStats.SpDefence = statFile.ReadByte();

                    type1 = Types[statFile.ReadByte()];
                    type2 = Types[statFile.ReadByte()];

                    catchRate = statFile.ReadByte();
                    expYield = statFile.ReadByte();

                    var evBinary = statFile.ReadUInt16();
                    evYield.HP = evBinary & 3;
                    evYield.Attack = (evBinary >> 2) & 3;
                    evYield.Defence = (evBinary >> 4) & 3;
                    evYield.Speed = (evBinary >> 6) & 3;
                    evYield.SpAttack = (evBinary >> 8) & 3;
                    evYield.SpDefence = (evBinary >> 10) & 3;

                    statFile.ReadUInt16();//Item 50%
                    statFile.ReadUInt16();//Item 5%

                    var genderByte = statFile.ReadByte();
                    if (genderByte <= 254)
                        genderChance = genderByte / 254f;

                    hatchingSteps = statFile.ReadByte() * 255;

                    baseFriendship = statFile.ReadByte();

                    expType = (EExpType)statFile.ReadByte();

                    eggGroup1 = statFile.ReadByte();
                    eggGroup2 = statFile.ReadByte();

                    statFile.ReadByte();//Ability1
                    statFile.ReadByte();//Ability2
                }
                _monsters.Add(new MonsterSpecies(i, names[i], baseStats, type1, type2,
                    catchRate, expYield, evYield, genderChance, hatchingSteps, baseFriendship, 
                    expType, eggGroup1, eggGroup2));
            }
        }
    }
}
