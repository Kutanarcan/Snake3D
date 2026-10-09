using Game.Core;

namespace Game.Tests.EditMode
{
    // Hands back whatever slot the test asks for and records the range it was
    // offered, which is how the uniformity of the pick gets checked.
    public sealed class ScriptedRandomSource : IRandomSource
    {
        public int NextValue { get; set; }

        public int LastMaxExclusive { get; private set; }

        public int LastSeed { get; private set; }

        public int Next(int maxExclusive)
        {
            LastMaxExclusive = maxExclusive;
            return NextValue;
        }

        // The scripted answer does not come from a seed, so reseeding only leaves
        // a trace a test can assert on.
        public void Reseed(int seed)
        {
            LastSeed = seed;
        }
    }
}
