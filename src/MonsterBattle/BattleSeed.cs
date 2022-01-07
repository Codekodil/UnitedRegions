using MonsterData;

namespace MonsterBattle
{
    public class BattleSeed
    {
        public MonsterBox PlayerTeam { get; }
        public MonsterBox OpponentTeam { get; }

        public BattleSeed(MonsterBox playerTeam, MonsterBox opponentTeam)
        {
            PlayerTeam = playerTeam;
            OpponentTeam = opponentTeam;
        }
    }
}
