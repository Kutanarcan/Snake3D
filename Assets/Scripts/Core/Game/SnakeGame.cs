using System;

namespace Game.Core
{
    public sealed class SnakeGame
    {
        private readonly GridMap _map;
        private readonly Snake _snake;
        private readonly AppleSpawnManager _apples;
        private readonly SnakeCells _snakeCells;
        private GrowthDebt _growth;

        public SnakeGame(GridMap map, Snake snake, AppleSpawnManager apples)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (snake == null)
            {
                throw new ArgumentNullException(nameof(snake));
            }

            if (apples == null)
            {
                throw new ArgumentNullException(nameof(apples));
            }

            if (snake.Capacity < map.Size.CellCount)
            {
                throw new ArgumentException("The snake must be able to grow into every cell of the grid.", nameof(snake));
            }

            _map = map;
            _snake = snake;
            _apples = apples;
            _snakeCells = new SnakeCells(map, snake);
        }

        public GridMap Map
        {
            get { return _map; }
        }

        public Snake Snake
        {
            get { return _snake; }
        }

        public AppleSpawnManager Apples
        {
            get { return _apples; }
        }

        public FoodSpawner Food
        {
            get { return _apples.Plain; }
        }

        // What the last step ate, or Empty when it ate nothing. The run reads it
        // to score the bite and to tell a bonus apple from the plain one.
        public CellOccupantType LastMeal { get; private set; }

        // How the last step ended the run, or None when it did not.
        public DeathCause LastDeath { get; private set; }

        // The cell that stopped the head. Only worth reading while LastDeath
        // names a cause -- and when that cause is Bounds the cell lies outside
        // the board, so it is no good asking the map about it.
        public GridPosition LastDeathCell { get; private set; }

        // Whether the last step ended on a tunnel and was turned by it. The
        // deflection is a moment the player sees, so it is reported rather than
        // left to be inferred from the direction changing.
        public bool LastStepDeflected { get; private set; }

        public bool TryTurn(Direction direction)
        {
            return _snake.TryTurn(direction);
        }

        public void Feed(int cells)
        {
            _growth.Add(cells);
        }

        public void Reset(GridPosition headPos, Direction direction, int length)
        {
            _snakeCells.Place(headPos, direction, length);
            _growth.Clear();
            Forget();

            _apples.Restart();
        }

        public StepResult Step()
        {
            Forget();

            GridPosition nextPos = _snake.PeekNextHead();
            DeathCause cause = CauseOfDeathAt(nextPos);

            if (cause != DeathCause.None)
            {
                LastDeath = cause;
                LastDeathCell = nextPos;

                return StepResult.Died;
            }

            LastMeal = _map[nextPos];
            int cells = AppleSpawnManager.ValueOf(LastMeal);

            if (cells > 0)
            {
                Feed(cells);
            }

            Advance(nextPos);
            _apples.Take(LastMeal);

            return cells > 0 ? StepResult.Ate : StepResult.Moved;
        }

        private void Advance(GridPosition nextPos)
        {
            GridPosition tailPos = _snake.Tail;
            bool willGrow = _growth.Pay();

            _snake.Step(willGrow);

            if (!willGrow)
            {
                _map.Set(tailPos, CellOccupantType.Empty);
            }

            _map.Set(nextPos, CellOccupantType.Snake);

            if (!GroundRules.Deflects(_map.GroundAt(nextPos)))
            {
                return;
            }

            _snake.Force(_snake.Direction.Deflect());
            LastStepDeflected = true;
        }

        // Naming what is fatal about the cell rather than answering yes or no is
        // the whole of it: the run cannot tell a wall from a body otherwise.
        private DeathCause CauseOfDeathAt(GridPosition nextPos)
        {
            if (!_map.Size.Contains(nextPos))
            {
                return DeathCause.Bounds;
            }

            if (GroundRules.Kills(_map.GroundAt(nextPos)))
            {
                return DeathCause.Ground;
            }

            if (_map[nextPos] != CellOccupantType.Snake)
            {
                return DeathCause.None;
            }

            return nextPos != _snake.Tail || _growth.IsOwed ? DeathCause.Snake : DeathCause.None;
        }

        private void Forget()
        {
            LastMeal = CellOccupantType.Empty;
            LastDeath = DeathCause.None;
            LastStepDeflected = false;
        }
    }
}
