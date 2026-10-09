using System;

namespace Game.Core
{
    // Sparse Set
    internal struct FreeCellSet
    {
        private readonly int[] _freeCells;
        private readonly int[] _slotOf;
        private int _count;

        public FreeCellSet(int cellCount)
        {
            if (cellCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellCount), "Cell count must be positive.");
            }

            _freeCells = new int[cellCount];
            _slotOf = new int[cellCount];
            _count = 0;
            Fill();
        }

        public int Count
        {
            get { return _count; }
        }

        public int this[int slot]
        {
            get
            {
                if (slot < 0 || slot >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(slot));
                }

                return _freeCells[slot];
            }
        }

        public void Add(int cell)
        {
            _slotOf[cell] = _count;
            _freeCells[_count] = cell;
            _count++;
        }

        public void Remove(int cell)
        {
            int slot = _slotOf[cell];
            _count--;

            int moved = _freeCells[_count];
            _freeCells[slot] = moved;
            _slotOf[moved] = slot;

            _slotOf[cell] = -1;
        }

        public void Fill()
        {
            _count = 0;
            for (int cell = 0; cell < _freeCells.Length; cell++)
            {
                Add(cell);
            }
        }
    }
}
