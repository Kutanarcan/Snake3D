using System;

namespace Game.Core
{
    public static class DirectionExtensions
    {
        public static GridPosition ToOffset(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new GridPosition(0, 1);
                case Direction.Right:
                    return new GridPosition(1, 0);
                case Direction.Down:
                    return new GridPosition(0, -1);
                case Direction.Left:
                    return new GridPosition(-1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        public static bool TryFromVector(float x, float y, float deadzone, out Direction direction)
        {
            direction = Direction.Up;

            float horizontal = x < 0f ? -x : x;
            float vertical = y < 0f ? -y : y;

            if (horizontal < deadzone && vertical < deadzone)
            {
                return false;
            }

            if (horizontal > vertical)
            {
                direction = x > 0f ? Direction.Right : Direction.Left;
                return true;
            }

            if (vertical > horizontal)
            {
                direction = y > 0f ? Direction.Up : Direction.Down;
                return true;
            }

            return false;
        }

        public static Direction Deflect(this Direction direction)
        {
            return (Direction)((int)direction ^ 1);
        }

        public static Direction Opposite(this Direction direction)
        {
            return (Direction)(((int)direction + 2) & 3);
        }

        public static bool IsOpposite(this Direction direction, Direction other)
        {
            return direction.Opposite() == other;
        }
    }
}
