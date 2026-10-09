using System;

namespace Game.Core
{
    internal struct GrowthDebt
    {
        private int _owed;

        internal bool IsOwed
        {
            get { return _owed > 0; }
        }

        internal void Add(int cells)
        {
            if (cells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cells), "A food must be worth at least one cell.");
            }

            _owed += cells;
        }

        internal bool Pay()
        {
            if (_owed == 0)
            {
                return false;
            }

            _owed--;
            return true;
        }

        internal void Clear()
        {
            _owed = 0;
        }
    }
}
