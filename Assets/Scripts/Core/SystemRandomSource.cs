using System;

namespace Game.Core
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private Random _random;

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }

        public void Reseed(int seed)
        {
            _random = new Random(seed);
        }
    }
}
