using System;

namespace Game.Core
{
    // The body seen from the map: which cells it claims, which it gives back,
    // and whether a proposed layout has room at all.
    public sealed class SnakeCells
    {
        private readonly GridMap _map;
        private readonly Snake _snake;

        public SnakeCells(GridMap map, Snake snake)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (snake == null)
            {
                throw new ArgumentNullException(nameof(snake));
            }

            _map = map;
            _snake = snake;
        }

        public void Place(GridPosition headPos, Direction direction, int length)
        {
            Release();

            Result room = CanOccupy(headPos, direction, length);
            if (!room.IsSuccess)
            {
                throw new ArgumentException(room.Error);
            }

            _snake.Reset(headPos, direction, length);
            Occupy();
        }

        public void Occupy()
        {
            for (int i = 0; i < _snake.Length; i++)
            {
                _map.Set(_snake[i], CellOccupantType.Snake);
            }
        }

        public void Release()
        {
            for (int i = 0; i < _snake.Length; i++)
            {
                _map.Set(_snake[i], CellOccupantType.Empty);
            }
        }

        public Result CanOccupy(GridPosition headPos, Direction direction, int length)
        {
            GridPosition backwards = direction.Opposite().ToOffset();
            GridPosition cellPos = headPos;

            for (int i = 0; i < length; i++)
            {
                if (!_map.Size.Contains(cellPos))
                {
                    return Result.Failure("The snake would start outside the grid at " + cellPos + ".");
                }

                if (!_map.IsFree(cellPos))
                {
                    return Result.Failure("The snake would start on a " + BlockerAt(cellPos) + " cell at " + cellPos + ".");
                }

                cellPos += backwards;
            }

            return Result.Success();
        }

        private string BlockerAt(GridPosition position)
        {
            CellGroundType ground = _map.GroundAt(position);

            return ground != CellGroundType.Open ? ground.ToString() : _map[position].ToString();
        }
    }
}
