namespace Squizzy.Entities
{
    public class MaxTimeUpgrade : MagnetUpgrade
    {
        public MaxTimeUpgrade()
            : base(1, "Maximum Answer Time", 250, 250)
        {
        }

        public int CalculateMaxTime(int level, int maxSeconds) => maxSeconds + (maxSeconds / 20 * level); //5% per level
    }
}
