namespace Squizzy.Entities
{
    public abstract class MagnetUpgrade : IUpgrade
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string[] NameShortcuts { get; }
        public abstract int BaseCost { get; }
        public abstract int CostStep { get; }

        public int GetCost(int level)
            => BaseCost + (CostStep * level);
        public abstract string GetCurrentValue(int level);
    }
}
