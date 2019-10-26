namespace Squizzy.Entities
{
    public class PerfectTimeUpgrade : MagnetUpgrade
    {
        public override int Id => 1;

        public override string Name => "Perfect Time";

        public override string Description => "Increases the time you have to get a perfect answer by 0.1 seconds";

        public override string[] NameShortcuts => new[] { Name, "PerfectTime", "PTime", "PerfectDuration", "PDuration", "PSpan" };

        public override int BaseCost => 1000;

        public override int CostStep => 1000;

        public override bool RequireRecalculation => true;


        public override string GetCurrentValue(int level) => $"{CalculateValue(level)} seconds";
        public double CalculateValue(int level) => 1.6d + (0.1d * level);
    }
}
