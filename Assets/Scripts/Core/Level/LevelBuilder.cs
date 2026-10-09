using System;

namespace Game.Core
{
    public static class LevelBuilder
    {
        public static GameSession Build(Level level, float tickInterval, IRandomSource random, IInputSource input)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            GridMap map = new GridMap(level.Board);
            Paint(level, map);

            Snake snake = new Snake(level.Board.CellCount);
            AppleSpawnManager apples = new AppleSpawnManager(new FoodSpawner(map, random), Golden(level, map, random, tickInterval));

            return new GameSession(new SnakeGame(map, snake, apples), new TickTimer(tickInterval), input, level);
        }

        private static GoldenAppleSpawner Golden(Level level, GridMap map, IRandomSource random, float tickInterval)
        {
            GoldenAppleRule rule = level.GoldenApple;

            return new GoldenAppleSpawner(map, random, rule.SpawnTicks(tickInterval), rule.StayTicks(tickInterval));
        }

        private static void Paint(Level level, GridMap map)
        {
            GridSize board = level.Board;

            for (int cell = 0; cell < board.CellCount; cell++)
            {
                GridPosition position = board.FromIndex(cell);
                CellGroundType type = level.GroundAt(position);

                if (type != CellGroundType.Open)
                {
                    map.SetGround(position, type);
                }
            }
        }
    }
}
