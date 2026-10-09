using System;

namespace Game.Core
{
    // Whether a level's numbers describe a run that can actually be played on
    // the board the layout drew.
    internal static class LevelValidator
    {
        internal static Result ValidateBoard(LevelData data, out GridSize board)
        {
            board = default(GridSize);

            if (data.Width <= 0 || data.Width > Level.MaxSide)
            {
                return Result.Failure("The board is " + data.Width + " wide; width must be between 1 and " + Level.MaxSide + ".");
            }

            if (data.Height <= 0 || data.Height > Level.MaxSide)
            {
                return Result.Failure("The board is " + data.Height + " tall; height must be between 1 and " + Level.MaxSide + ".");
            }

            board = new GridSize(data.Width, data.Height);

            return Result.Success();
        }

        internal static Result Validate(LevelData data, GridSize board, CellGroundType[] ground, out Direction direction)
        {
            direction = Direction.Up;

            Result runResult = ValidateRun(data);
            if (!runResult.IsSuccess)
            {
                return runResult;
            }

            Result roomResult = ValidateRoom(ground, data.SnakeLength, data.FoodGoal);
            if (!roomResult.IsSuccess)
            {
                return roomResult;
            }

            Result directionResult = ParseDirection(data.StartDirection, out direction);
            if (!directionResult.IsSuccess)
            {
                return directionResult;
            }

            return StartPlacement.Fits(board, ground,
                new GridPosition(data.StartX, data.StartY), direction, data.SnakeLength);
        }

        private static Result ValidateRun(LevelData data)
        {
            if (data.SnakeLength < 1)
            {
                return Result.Failure("The snake starts with " + data.SnakeLength + " cells; it needs at least one.");
            }

            if (data.FoodGoal < 0)
            {
                return Result.Failure("The food goal is " + data.FoodGoal + "; use 0 for an endless run.");
            }

            return Result.Success();
        }

        // Walls shrink the board the run actually has, so the room a level needs
        // is measured against the open cells rather than against every cell.
        private static Result ValidateRoom(CellGroundType[] ground, int length, int foodGoal)
        {
            int open = OpenCells(ground);

            if (length >= open)
            {
                return Result.Failure("The snake fills all " + open + " open cells, leaving nowhere for food.");
            }

            if (foodGoal > 0 && length + foodGoal > open)
            {
                return Result.Failure("The board leaves " + open + " open cells; a " + length
                    + "-cell snake cannot grow by " + foodGoal + " food on it.");
            }

            return Result.Success();
        }

        private static int OpenCells(CellGroundType[] ground)
        {
            int open = 0;

            for (int cell = 0; cell < ground.Length; cell++)
            {
                if (ground[cell] == CellGroundType.Open)
                {
                    open++;
                }
            }

            return open;
        }

        private static Result ParseDirection(string text, out Direction direction)
        {
            direction = Direction.Up;

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure("The snake needs a start direction: Up, Right, Down or Left.");
            }

            // Enum.TryParse also accepts "7", so a defined name is checked separately.
            if (!Enum.TryParse<Direction>(text, true, out direction)
                || !Enum.IsDefined(typeof(Direction), direction))
            {
                direction = Direction.Up;
                return Result.Failure("\"" + text + "\" is not a direction; use Up, Right, Down or Left.");
            }

            return Result.Success();
        }
    }
}
