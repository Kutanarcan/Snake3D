using System;

namespace Game.Core
{
    public sealed class AppleSpawnManager
    {
        private readonly FoodSpawner _plain;
        private readonly GoldenAppleSpawner _golden;

        public AppleSpawnManager(FoodSpawner plain, GoldenAppleSpawner golden)
        {
            if (plain == null)
            {
                throw new ArgumentNullException(nameof(plain));
            }

            if (golden == null)
            {
                throw new ArgumentNullException(nameof(golden));
            }

            _plain = plain;
            _golden = golden;
        }

        public FoodSpawner Plain
        {
            get { return _plain; }
        }

        public GoldenAppleSpawner Golden
        {
            get { return _golden; }
        }

        public static int ValueOf(CellOccupantType meal)
        {
            switch (meal)
            {
                case CellOccupantType.Food:
                    return 1;
                case CellOccupantType.GoldenApple:
                    return GoldenAppleSpawner.Value;
                default:
                    return 0;
            }
        }

        public void Restart()
        {
            _golden.Despawn();
            _plain.TryRespawn();
        }
        public void Take(CellOccupantType meal)
        {
            if (meal == CellOccupantType.Food)
            {
                _plain.Despawn();
            }
            else if (meal == CellOccupantType.GoldenApple)
            {
                _golden.Despawn();
            }

            _golden.Tick();
        }
    }
}
