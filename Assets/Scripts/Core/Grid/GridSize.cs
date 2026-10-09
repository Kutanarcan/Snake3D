using System;

namespace Game.Core
{
    public readonly struct GridSize
    {
        public readonly int Width;
        public readonly int Height;

        public GridSize(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Grid width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Grid height must be positive.");
            }

            Width = width;
            Height = height;
        }

        public int CellCount
        {
            get { return Width * Height; }
        }

        public bool Contains(GridPosition position)
        {
            return position.X >= 0 && position.X < Width
                && position.Y >= 0 && position.Y < Height;
        }

        public int ToIndex(GridPosition position)
        {
            return position.Y * Width + position.X;
        }

        public GridPosition FromIndex(int index)
        {
            return new GridPosition(index % Width, index / Width);
        }
    }
}
