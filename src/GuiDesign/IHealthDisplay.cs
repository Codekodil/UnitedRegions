namespace GuiDesign
{
    public interface IHealthDisplay
    {
        string MonsterName { get; set; }
        int Level { get; set; }
        int CurrentHp { get; set; }
        int MaxHp { get; set; }
        float OffScreen { get; set; }
    }
}
