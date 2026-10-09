using System;
using Game.Core;

namespace Game.Runtime
{
    public sealed class SnakeTrail
    {
        private readonly Snake _snake;

        private GridPosition _lastHead;
        private GridPosition _lastTail;
        private GridPosition _tailBuffer;
        private int _lastLength;
        private bool _sliding;
        private bool _tailMoved;

        public SnakeTrail(Snake snake)
        {
            if (snake == null)
            {
                throw new ArgumentNullException(nameof(snake));
            }

            _snake = snake;
        }

        public int Length
        {
            get { return _snake.Length; }
        }

        public void Restart()
        {
            _sliding = false;
            _tailMoved = false;

            StoreCurrentStates();
        }
        public void Sync()
        {
            if (_snake.Head != _lastHead)
            {
                _tailBuffer = _lastTail;
                _tailMoved = _snake.Length == _lastLength;
                _sliding = IsNextTo(_lastHead, _snake.Head);
            }

            StoreCurrentStates();
        }

        public GridPosition To(int index)
        {
            return _snake[index];
        }

        public GridPosition From(int index)
        {
            if (!_sliding)
            {
                return _snake[index];
            }

            if (index < _snake.Length - 1)
            {
                return _snake[index + 1];
            }

            return _tailMoved ? _tailBuffer : _snake[index];
        }

        public Direction Facing(int index)
        {
            if (index == 0)
            {
                return HeadFacing();
            }

            GridPosition ahead = _snake[index - 1];
            GridPosition here = _snake[index];

            Direction facing;
            DirectionExtensions.TryFromVector(ahead.X - here.X, ahead.Y - here.Y, 0.5f, out facing);

            return facing;
        }

        // The head is drawn facing the way it is travelling this step, not the
        // way it will leave. A tunnel turns it on arrival, and reading the new
        // heading here would slide it sideways into the cell it is entering.
        private Direction HeadFacing()
        {
            GridPosition from = From(0);
            GridPosition to = To(0);

            Direction moving;
            if (DirectionExtensions.TryFromVector(to.X - from.X, to.Y - from.Y, 0.5f, out moving))
            {
                return moving;
            }

            // Standing still: the first frame of a run has nowhere to read a
            // heading from but the snake itself.
            return _snake.Direction;
        }

        private static bool IsNextTo(GridPosition a, GridPosition b)
        {
            int across = a.X > b.X ? a.X - b.X : b.X - a.X;
            int along = a.Y > b.Y ? a.Y - b.Y : b.Y - a.Y;

            return across + along == 1;
        }

        private void StoreCurrentStates()
        {
            if (_snake.Length == 0)
            {
                return;
            }

            _lastHead = _snake.Head;
            _lastLength = _snake.Length;
            _lastTail = _snake[_snake.Length - 1];
        }
    }
}
