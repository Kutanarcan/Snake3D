using System;

namespace Game.Core
{
    public readonly struct CellStock
    {
        private CellStock(int environment, int organic, int ground)
        {
            Environment = environment;
            Organic = organic;
            Ground = ground;
        }

        public int Environment { get; }
        public int Organic { get; }
        public int Ground { get; }

        public static CellStock For(LevelSequence levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            int environment = 0;
            int organic = 0;
            int ground = 0;

            for (int i = 0; i < levels.Count; i++)
            {
                Level level = levels[i];

                environment = Math.Max(environment, level.Board.CellCount);
                organic = Math.Max(organic, OrganicFor(level));
                ground = Math.Max(ground, level.GroundCount);
            }

            return new CellStock(environment, organic, ground);
        }

        private static int OrganicFor(Level level)
        {
            return level.FoodGoal > 0
                ? level.SnakeLength + level.FoodGoal + 2
                : level.Board.CellCount;
        }
    }
}
