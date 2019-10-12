namespace Squizzy.Entities
{
    public abstract class MagnetUpgrade : IUpgrade
    {
        public int Id { get; }
        public string Name { get; }
        public int BaseCost { get; }
        public int CostStep { get; }

        public MagnetUpgrade(int id, string name, int baseCost, int costStep)
        {
            Id = id;
            Name = name;
            BaseCost = baseCost;
            CostStep = costStep;
        }
    }
}
