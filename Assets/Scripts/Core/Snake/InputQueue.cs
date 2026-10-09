namespace Game.Core
{
    public sealed class InputQueue
    {
        public const int Depth = 2;

        private readonly Direction[] _pending = new Direction[Depth];
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public bool IsEmpty
        {
            get { return _count == 0; }
        }

        public bool IsFull
        {
            get { return _count == Depth; }
        }

        public Direction Front
        {
            get { return _pending[0]; }
        }

        public Direction Back
        {
            get { return _pending[_count - 1]; }
        }

        public bool TryEnqueue(Direction direction)
        {
            if (IsFull)
            {
                return false;
            }

            _pending[_count] = direction;
            _count++;

            return true;
        }

        public bool TryDequeue(out Direction direction)
        {
            if (_count == 0)
            {
                direction = Direction.Up;

                return false;
            }

            direction = _pending[0];
            _count--;

            for (int i = 0; i < _count; i++)
            {
                _pending[i] = _pending[i + 1];
            }

            return true;
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}
