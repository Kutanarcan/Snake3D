using Game.Core;

namespace Game.Authoring
{
    public readonly struct BoardToGrid
    {
        private readonly GridSize _board;

        public BoardToGrid(GridSize board)
        {
            _board = board;
        }

        public int Rows
        {
            get { return _board.Width; }
        }

        public int Columns
        {
            get { return _board.Height; }
        }

        public GridPosition Cell(int row, int column)
        {
            return new GridPosition(_board.Width - 1 - row, _board.Height - 1 - column);
        }

        public int RowOf(GridPosition cell)
        {
            return _board.Width - 1 - cell.X;
        }

        public int ColumnOf(GridPosition cell)
        {
            return _board.Height - 1 - cell.Y;
        }

        public static Direction DrawnAs(Direction heading)
        {
            return (Direction)(((int)heading + 3) & 3);
        }

        public static Direction Heading(Direction drawnAs)
        {
            return (Direction)(((int)drawnAs + 1) & 3);
        }
    }
}
