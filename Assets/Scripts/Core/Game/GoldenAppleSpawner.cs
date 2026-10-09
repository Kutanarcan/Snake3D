using System;

namespace Game.Core
{
    public sealed class GoldenAppleSpawner
    {
        public const int Value = 3;

        private readonly GridMap _map;
        private readonly IRandomSource _random;
        private readonly int _spawnDelay;
        private readonly int _stay;

        private GridPosition _position;
        private int _countdown;

        public GoldenAppleSpawner(GridMap map, IRandomSource random, int spawnDelay, int stay)
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
            _spawnDelay = spawnDelay;
            _stay = stay;
            _countdown = spawnDelay;
        }

        public bool IsEnabled
        {
            get { return _spawnDelay > 0 && _stay > 0; }
        }

        public bool IsOnTheBoard { get; private set; }

        public GridPosition Position
        {
            get { return _position; }
        }

        public void Tick()
        {
            if (!IsEnabled)
            {
                return;
            }

            _countdown--;

            if (_countdown > 0)
            {
                return;
            }

            if (IsOnTheBoard)
            {
                Despawn();
                return;
            }

            _countdown = TrySpawn() ? _stay : 1;
        }

        public void Despawn()
        {
            if (IsOnTheBoard && _map[_position] == CellOccupantType.GoldenApple)
            {
                _map.Set(_position, CellOccupantType.Empty);
            }

            IsOnTheBoard = false;
            _countdown = _spawnDelay;
        }

        private bool TrySpawn()
        {
            if (_map.FreeCount == 0)
            {
                return false;
            }

            _position = _map.FreeCellAt(_random.Next(_map.FreeCount));
            _map.Set(_position, CellOccupantType.GoldenApple);
            IsOnTheBoard = true;

            return true;
        }
    }
}
