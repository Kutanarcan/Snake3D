using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Authoring
{
    public sealed class ItemGrid
    {
        private LevelItemData[] _cells;
        private GridSize _board;

        public ItemGrid(GridSize board)
        {
            _board = board;
            _cells = new LevelItemData[board.CellCount];
        }

        public GridSize Board
        {
            get { return _board; }
        }

        public LevelItemData ItemAt(GridPosition position)
        {
            return _cells[Cell(position)];
        }

        public void Paint(GridPosition position, CellGroundType type)
        {
            if (type == CellGroundType.Open)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Open puts nothing down; erase the cell instead.");
            }

            _cells[Cell(position)] = new LevelItemData
            {
                Type = type.ToString(),
                X = position.X,
                Y = position.Y
            };
        }

        public void Erase(GridPosition position)
        {
            _cells[Cell(position)] = null;
        }

        public void Resize(GridSize board)
        {
            LevelItemData[] kept = new LevelItemData[board.CellCount];

            for (int cell = 0; cell < _cells.Length; cell++)
            {
                LevelItemData item = _cells[cell];

                if (item == null)
                {
                    continue;
                }

                GridPosition position = new GridPosition(item.X, item.Y);

                if (board.Contains(position))
                {
                    kept[board.ToIndex(position)] = item;
                }
            }

            _cells = kept;
            _board = board;
        }

        public LevelItemData[] ToItems()
        {
            List<LevelItemData> items = new List<LevelItemData>();

            for (int cell = 0; cell < _cells.Length; cell++)
            {
                if (_cells[cell] != null)
                {
                    items.Add(_cells[cell]);
                }
            }

            return items.ToArray();
        }

        public static ItemGrid From(LevelItemData[] items, GridSize board)
        {
            ItemGrid grid = new ItemGrid(board);

            if (items == null)
            {
                return grid;
            }

            for (int i = 0; i < items.Length; i++)
            {
                grid.Place(items[i], i);
            }

            return grid;
        }

        private void Place(LevelItemData item, int index)
        {
            if (item == null)
            {
                throw new ArgumentException("Item " + (index + 1) + " is missing.", nameof(item));
            }

            int cell = Cell(new GridPosition(item.X, item.Y));

            if (_cells[cell] != null)
            {
                throw new ArgumentException("Item " + (index + 1) + " lands on ("
                    + item.X + ", " + item.Y + "), which is already taken.");
            }

            _cells[cell] = item;
        }

        private int Cell(GridPosition position)
        {
            if (!_board.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position + " is outside the board.");
            }

            return _board.ToIndex(position);
        }
    }
}
