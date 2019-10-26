namespace Squizzy.Entities
{
    public class PerfectTrophiesUpgrade : MagnetUpgrade
    {
        public override int Id => 2;

        public override string Name => "Perfect Trophies";

        public override string Description => "Increases the :trophy: you get for a perfect answered question by 1";

        public override string[] NameShortcuts => new[] { Name, "PerfectTrophies", "PerfectTrophs", "TrophiesPerfect", "PerfectTrophy", "PTrophies", "PTroph", "PTrop" };

        public override int BaseCost => 2000;

        public override int CostStep => 1500;

        public override bool RequireRecalculation => true;


        public override string GetCurrentValue(int level) => $"+{CalculateValue(level, 0)} :trophy: for a perfect answer";
        public int CalculateValue(int level, int perfectTrophies) => perfectTrophies + level;
    }
}
