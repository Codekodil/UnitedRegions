namespace MonsterData
{
    public class DamageType
    {
        public int Id { get; }
        public string Name { get; }
        internal DamageType(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => "[" + Name.Substring(0, 1) + Name.Substring(1).ToLower() + "]";
    }
}
