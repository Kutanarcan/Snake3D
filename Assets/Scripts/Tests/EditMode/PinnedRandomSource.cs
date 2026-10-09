using System;
using Game.Core;

namespace Game.Tests.EditMode
{
    // Steers the pick to one chosen cell, so a test can say where the food lands
    // instead of guessing which free list slot happens to hold it.
    public sealed class PinnedRandomSource : IRandomSource
    {
        private readonly GridMap _map;

        public PinnedRandomSource(GridMap map, GridPosition target)
        {
            _map = map;
            Target = target;
        }

        public GridPosition Target { get; set; }

        public int Next(int maxExclusive)
        {
            for (int slot = 0; slot < maxExclusive; slot++)
            {
                if (_map.FreeCellAt(slot) == Target)
                {
                    return slot;
                }
            }

            throw new InvalidOperationException(Target + " is not free, so the food cannot be pinned there.");
        }

        // The target is fixed by the test, so a seed has nothing to change here.
        public void Reseed(int seed)
        {
        }
    }
}
