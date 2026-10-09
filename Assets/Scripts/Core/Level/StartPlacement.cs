namespace Game.Core
{
    // Whether the body a level asks for fits where the level puts it: on the
    // board, and on ground it can lie on.
    internal static class StartPlacement
    {
        internal static Result Fits(GridSize board, CellGroundType[] ground, GridPosition head,
            Direction direction, int length)
        {
            Result boundsResult = WithinTheBoard(board, head, direction, length);
            if (!boundsResult.IsSuccess)
            {
                return boundsResult;
            }

            return OnOpenGround(board, ground, head, direction, length);
        }

        private static Result WithinTheBoard(GridSize board, GridPosition head, Direction direction, int length)
        {
            if (!board.Contains(head))
            {
                return Result.Failure("The snake starts at " + head + ", which is off the board.");
            }

            // The body trails behind the head, so only its far end can fall off.
            GridPosition step = direction.Opposite().ToOffset();
            GridPosition tail = new GridPosition(head.X + step.X * (length - 1), head.Y + step.Y * (length - 1));

            if (!board.Contains(tail))
            {
                return Result.Failure("A " + length + "-cell snake facing " + direction + " from " + head
                    + " would put its tail at " + tail + ", off the board.");
            }

            return Result.Success();
        }

        // Head and tail are both on the board by now, so every cell between them
        // is too and the walk needs no bounds check of its own.
        private static Result OnOpenGround(GridSize board, CellGroundType[] ground, GridPosition head,
            Direction direction, int length)
        {
            GridPosition backwards = direction.Opposite().ToOffset();
            GridPosition cellPos = head;

            for (int i = 0; i < length; i++)
            {
                CellGroundType type = ground[board.ToIndex(cellPos)];

                if (type != CellGroundType.Open)
                {
                    return Result.Failure("A " + length + "-cell snake facing " + direction + " from " + head
                        + " would lie across a " + type + " at " + cellPos + ".");
                }

                cellPos += backwards;
            }

            return Result.Success();
        }
    }
}
