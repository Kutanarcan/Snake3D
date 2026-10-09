namespace Game.Core
{
    public interface IRandomSource
    {
        int Next(int maxExclusive);
        void Reseed(int seed);
    }
}
