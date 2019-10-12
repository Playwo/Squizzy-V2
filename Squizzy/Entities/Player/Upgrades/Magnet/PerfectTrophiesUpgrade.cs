namespace Squizzy.Entities
{
    public class PerfectTrophiesUpgrade : MagnetUpgrade
    {
        public PerfectTrophiesUpgrade()
            : base(3, "Perfect Trophies", 2000, 1500)
        {
        }

        public int CalculatePerfectTrophies(int level, int perfectTrophies) => perfectTrophies + level;
    }
}
