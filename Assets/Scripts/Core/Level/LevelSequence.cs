using System;

namespace Game.Core
{
    public sealed class LevelSequence
    {
        private readonly Level[] _levels;

        public LevelSequence(Level[] levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            if (levels.Length == 0)
            {
                throw new ArgumentException("A sequence has to have a level to start on.", nameof(levels));
            }

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                {
                    throw new ArgumentException("Level " + i + " of the sequence is missing.", nameof(levels));
                }
            }

            _levels = levels;
        }

        public int Count
        {
            get { return _levels.Length; }
        }

        public int Index { get; private set; }

        public Level Current
        {
            get { return _levels[Index]; }
        }

        public Level this[int index]
        {
            get { return _levels[index]; }
        }

        public void Advance()
        {
            Index = (Index + 1) % _levels.Length;
        }

        // A remembered place can outlive the list that produced it, so it lands
        // on the ring the same way Advance does rather than being refused.
        public void GoTo(int index)
        {
            Index = ((index % _levels.Length) + _levels.Length) % _levels.Length;
        }
    }
}
