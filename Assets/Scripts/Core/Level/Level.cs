using System;

namespace Game.Core
{
    public sealed class Level
    {
        public const int MaxSide = 128;

        private readonly CellGroundType[] _ground;

        internal Level(GridSize board, CellGroundType[] ground, GridPosition snakeStart,
            Direction snakeDirection, int snakeLength, int foodGoal, GoldenAppleRule goldenApple)
        {
            Board = board;
            _ground = ground;
            SnakeStart = snakeStart;
            SnakeDirection = snakeDirection;
            SnakeLength = snakeLength;
            FoodGoal = foodGoal;
            GoldenApple = goldenApple;
            GroundCount = CountGround(ground);
        }

        public GridSize Board { get; }
        public GridPosition SnakeStart { get; }
        public Direction SnakeDirection { get; }
        public int SnakeLength { get; }
        public int FoodGoal { get; }
        public GoldenAppleRule GoldenApple { get; }

        // How many cells carry something other than open ground. The view stock
        // is cut to this, so it is counted once here rather than per level load.
        public int GroundCount { get; }

        private static int CountGround(CellGroundType[] ground)
        {
            int count = 0;

            for (int cell = 0; cell < ground.Length; cell++)
            {
                if (ground[cell] != CellGroundType.Open)
                {
                    count++;
                }
            }

            return count;
        }

        public CellGroundType GroundAt(GridPosition position)
        {
            if (!Board.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position + " is outside the board.");
            }

            return _ground[Board.ToIndex(position)];
        }

        public static Result TryCreate(LevelData data, out Level level)
        {
            level = null;

            if (data == null)
            {
                return Result.Failure("A level needs data to be built from.");
            }

            GridSize board;
            CellGroundType[] ground;
            Direction direction;
            GoldenAppleRule goldenApple;

            Result readResult = Read(data, out board, out ground, out direction, out goldenApple);
            if (!readResult.IsSuccess)
            {
                return readResult;
            }

            level = new Level(board, ground, new GridPosition(data.StartX, data.StartY),
                direction, data.SnakeLength, data.FoodGoal, goldenApple);

            return Result.Success();
        }

        private static Result Read(LevelData data, out GridSize board, out CellGroundType[] ground,
            out Direction direction, out GoldenAppleRule goldenApple)
        {
            ground = null;
            direction = Direction.Up;
            goldenApple = GoldenAppleRule.None;

            Result boardResult = LevelValidator.ValidateBoard(data, out board);
            if (!boardResult.IsSuccess)
            {
                return boardResult;
            }

            Result itemsResult = LevelItems.TryRead(data.Items, board, out ground);
            if (!itemsResult.IsSuccess)
            {
                return itemsResult;
            }

            Result goldenResult = GoldenAppleRule.TryRead(data.GoldenApple, out goldenApple);
            if (!goldenResult.IsSuccess)
            {
                return goldenResult;
            }

            return LevelValidator.Validate(data, board, ground, out direction);
        }
    }
}
