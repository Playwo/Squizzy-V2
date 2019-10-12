namespace Squizzy.Entities
{
    public class MagnetGainUpgrade : MagnetUpgrade
    {
        public MagnetGainUpgrade()
            : base(0, "Magnet Gain", 250, 500)
        {
        }

        public int CalculateMagnetGain(int level) => 1 + level; //One more per level
    }
}
