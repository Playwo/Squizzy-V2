namespace Squizzy.Entities
{
    public class MagnetGainUpgrade : MagnetUpgrade
    {
        public override int Id => 0;

        public override string Name => "Magnet Gain";

        public override string Description => "Increases the amount of <:magnet:440898600738750465> you get per answered question by 1";

        public override string[] NameShortcuts => new[] { Name, "MagnetGain", "GainMagnets", "MagnetEarnings", "MGain", "MagnetG", "MG" };

        public override int BaseCost => 250;

        public override int CostStep => 500;

        public int CalculateValue(int level) => 1 + level; //1 + 1 per level
    }
}
