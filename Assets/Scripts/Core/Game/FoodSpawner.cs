using System;

namespace Game.Core
{
    public sealed class FoodSpawner
    {
        private readonly GridMap _map;
        private readonly IRandomSource _random;
        private GridPosition _position;

        public FoodSpawner(GridMap map, IRandomSource random)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            _map = map;
            _random = random;
        }

        public bool IsOnTheBoard { get; private set; }

        public GridPosition Position
        {
            get { return _position; }
        }

        public bool TryRespawn()
        {
            Despawn();

            if (_map.FreeCount == 0)
            {
                return false;
            }

            _position = _map.FreeCellAt(_random.Next(_map.FreeCount));
            _map.Set(_position, CellOccupantType.Food);
            IsOnTheBoard = true;

            return true;
        }

        public void Despawn()
        {
            if (IsOnTheBoard && _map[_position] == CellOccupantType.Food)
            {
                _map.Set(_position, CellOccupantType.Empty);
            }

            IsOnTheBoard = false;
        }
    }
}
