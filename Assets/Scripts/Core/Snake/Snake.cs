namespace Game.Core
{
    public sealed class Snake
    {
        private readonly SnakeBody _body;
        private Direction _direction;
        private readonly InputQueue _turns = new InputQueue();
        private bool _forced;

        public Snake(int capacity)
        {
            _body = new SnakeBody(capacity);
        }

        public int Capacity
        {
            get { return _body.Capacity; }
        }

        public int Length
        {
            get { return _body.Length; }
        }

        public Direction Direction
        {
            get { return _direction; }
        }

        public GridPosition Head
        {
            get { return _body.Head; }
        }

        public GridPosition Tail
        {
            get { return _body.Tail; }
        }

        public GridPosition this[int index]
        {
            get { return _body[index]; }
        }

        public void Reset(GridPosition head, Direction direction, int length)
        {
            _direction = direction;
            _turns.Clear();
            _forced = false;
            _body.Reset(head, direction, length);
        }

        public bool TryTurn(Direction direction)
        {
            if (_forced)
            {
                return false;
            }

            Direction pending = PendingDirection;

            if (direction.IsOpposite(pending))
            {
                return false;
            }

            if (direction == pending)
            {
                return true;
            }

            return _turns.TryEnqueue(direction);
        }

        public void Force(Direction direction)
        {
            _direction = direction;
            _turns.Clear();
            _forced = true;
        }

        public Direction PendingDirection
        {
            get { return _turns.IsEmpty ? _direction : _turns.Back; }
        }

        private Direction NextStepDirection
        {
            get { return _turns.IsEmpty ? _direction : _turns.Front; }
        }

        public GridPosition PeekNextHead()
        {
            return _body.Head + NextStepDirection.ToOffset();
        }

        public GridPosition Step(bool grow)
        {
            _forced = false;

            Direction turn;

            if (_turns.TryDequeue(out turn))
            {
                _direction = turn;
            }

            GridPosition next = _body.Head + _direction.ToOffset();
            _body.Advance(next, grow);
            return next;
        }

        public int IndexOf(GridPosition position)
        {
            return _body.IndexOf(position);
        }
    }
}
