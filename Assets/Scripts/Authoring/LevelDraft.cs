using System;
using Game.Core;

namespace Game.Authoring
{
    public sealed class LevelDraft
    {
        private readonly ItemGrid _items;

        public LevelDraft(GridSize board)
            : this(new ItemGrid(board))
        {
        }

        private LevelDraft(ItemGrid items)
        {
            _items = items;

            Start = new GridPosition(items.Board.Width / 2, items.Board.Height / 2);
            StartDirection = Direction.Right;
            SnakeLength = 3;
            FoodGoal = 5;
        }

        public ItemGrid Items
        {
            get { return _items; }
        }

        public GridSize Board
        {
            get { return _items.Board; }
        }

        public GridPosition Start { get; set; }
        public Direction StartDirection { get; set; }
        public int SnakeLength { get; set; }
        public int FoodGoal { get; set; }
        public GoldenAppleData GoldenApple { get; set; }

        public void Resize(GridSize board)
        {
            _items.Resize(board);
        }
        public void Resize(int width, int height)
        {
            Resize(new GridSize(WithinReach(width), WithinReach(height)));
        }

        private static int WithinReach(int side)
        {
            if (side < 1)
            {
                return 1;
            }

            return side > Level.MaxSide ? Level.MaxSide : side;
        }

        public static LevelDraft From(LevelData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            GridSize board = new GridSize(data.Width, data.Height);

            return new LevelDraft(ItemGrid.From(data.Items, board))
            {
                Start = new GridPosition(data.StartX, data.StartY),
                StartDirection = ParseDirection(data.StartDirection),
                SnakeLength = data.SnakeLength,
                FoodGoal = data.FoodGoal,
                GoldenApple = data.GoldenApple
            };
        }

        public LevelData ToData()
        {
            return new LevelData
            {
                Width = Board.Width,
                Height = Board.Height,
                StartX = Start.X,
                StartY = Start.Y,
                StartDirection = StartDirection.ToString(),
                SnakeLength = SnakeLength,
                FoodGoal = FoodGoal,
                Items = _items.ToItems(),
                GoldenApple = GoldenApple
            };
        }

        public Result Validate()
        {
            Level level;

            return Level.TryCreate(ToData(), out level);
        }

        private static Direction ParseDirection(string text)
        {
            Direction direction;

            if (!Enum.TryParse(text, true, out direction) || !Enum.IsDefined(typeof(Direction), direction))
            {
                return Direction.Up;
            }

            return direction;
        }
    }
}
