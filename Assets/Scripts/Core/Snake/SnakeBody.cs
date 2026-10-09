using System;

namespace Game.Core
{
    // Circular Array
    public sealed class SnakeBody
    {
        private readonly GridPosition[] _cells;
        private int _headIndex;

        public SnakeBody(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            }

            _cells = new GridPosition[capacity];
        }

        public int Capacity
        {
            get { return _cells.Length; }
        }

        public int Length { get; private set; }

        public GridPosition Head
        {
            get { return this[0]; }
        }

        public GridPosition Tail
        {
            get { return this[Length - 1]; }
        }

        public GridPosition this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _cells[(_headIndex + index) % _cells.Length];
            }
        }

        public void Reset(GridPosition head, Direction direction, int length)
        {
            if (length <= 0 || length > _cells.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be within capacity.");
            }

            _headIndex = 0;
            Length = length;

            GridPosition backwards = direction.Opposite().ToOffset();
            GridPosition cell = head;
            for (int i = 0; i < length; i++)
            {
                _cells[i] = cell;
                cell += backwards;
            }
        }

        public void Advance(GridPosition newHead, bool grow)
        {
            if (grow)
            {
                if (Length == _cells.Length)
                {
                    throw new InvalidOperationException("Snake body is already at capacity.");
                }

                Length++;
            }

            _headIndex = (_headIndex - 1 + _cells.Length) % _cells.Length;
            _cells[_headIndex] = newHead;
        }

        public int IndexOf(GridPosition position)
        {
            for (int i = 0; i < Length; i++)
            {
                if (_cells[(_headIndex + i) % _cells.Length] == position)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
