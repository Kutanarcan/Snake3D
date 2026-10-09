using Game.Core;

namespace Game.Runtime
{
    public static class RunEndings
    {
        public static EndingId For(SnakeGame game)
        {
            switch (game.LastDeath)
            {
                case DeathCause.Snake:
                    return EndingId.SelfBite;
                case DeathCause.Bounds:
                    return EndingId.OutOfBounds;
                default:
                    return EndingId.WallHit;
            }
        }
    }
}
