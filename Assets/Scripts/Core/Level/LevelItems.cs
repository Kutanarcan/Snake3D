using System;

namespace Game.Core
{
    internal static class LevelItems
    {
        internal static Result TryRead(LevelItemData[] items, GridSize board, out CellGroundType[] ground)
        {
            ground = new CellGroundType[board.CellCount];

            if (items == null)
            {
                return Result.Success();
            }

            for (int i = 0; i < items.Length; i++)
            {
                Result itemResult = Place(items[i], i, board, ground);
                if (!itemResult.IsSuccess)
                {
                    return itemResult;
                }
            }

            return Result.Success();
        }

        private static Result Place(LevelItemData item, int index, GridSize board, CellGroundType[] ground)
        {
            if (item == null)
            {
                return Result.Failure(Which(index) + " is missing.");
            }

            GridPosition position = new GridPosition(item.X, item.Y);

            if (!board.Contains(position))
            {
                return Result.Failure(Which(index) + " sits at " + position + ", which is off the board.");
            }

            CellGroundType type;

            Result typeResult = ParseType(item.Type, index, out type);
            if (!typeResult.IsSuccess)
            {
                return typeResult;
            }

            int cell = board.ToIndex(position);

            if (ground[cell] != CellGroundType.Open)
            {
                return Result.Failure(Which(index) + " lands on " + position + ", where a "
                    + ground[cell] + " already stands.");
            }

            ground[cell] = type;

            return Result.Success();
        }

        private static Result ParseType(string name, int index, out CellGroundType type)
        {
            type = CellGroundType.Open;

            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure(Which(index) + " does not say what it is.");
            }

            if (!Enum.TryParse<CellGroundType>(name, true, out type)
                || !Enum.IsDefined(typeof(CellGroundType), type))
            {
                type = CellGroundType.Open;
                return Result.Failure(Which(index) + " is a \"" + name + "\", which is not a cell type.");
            }

            if (type == CellGroundType.Open)
            {
                return Result.Failure(Which(index) + " is Open, which puts nothing on the board.");
            }

            return Result.Success();
        }

        private static string Which(int index)
        {
            return "Item " + (index + 1);
        }
    }
}
