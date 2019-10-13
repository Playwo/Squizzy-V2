namespace Squizzy.Entities
{
    public interface IUpgrade
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string[] NameShortcuts { get; }
    }
}
